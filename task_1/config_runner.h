#include <vector>
#include <future>
#include <iostream>
#include <algorithm>
#include <atomic>

#include "queue_on_list.h"
#include "producer.h"
#include "consumer.h"

// also used for producers' data generation in run_config
using Data_type     = int;
using Data_type_vec = std::vector<Data_type>;

class Config {
public:
    std::string name = "Unknown";

    int num_of_prods;
    int prod_elem_amount;
    std::chrono::seconds pause_for_producers;

    int num_of_cons;
    std::chrono::seconds pause_for_consumers;

    using Delay_function = void(*)();
    Delay_function delay_func;
};


bool run_config(const Config& cfg) {
    auto producers = std::vector<Producer<Data_type>>();
    producers.reserve(cfg.num_of_prods);

    for (Data_type i = 0; i < cfg.num_of_prods; ++i) {
        producers.emplace_back(Producer<Data_type>(
                Data_type_vec(cfg.prod_elem_amount, i), cfg.pause_for_producers));
    }

    auto consumers = std::vector<Consumer<Data_type>>();
    consumers.reserve(cfg.num_of_cons);

    for (Data_type i = 0; i < cfg.num_of_cons; ++i) {
        consumers.emplace_back(Consumer<Data_type>(cfg.pause_for_consumers));
    }

    auto queue = Queue_on_list<Data_type>();
    std::atomic<bool> should_stop = false;

    auto threads_for_producers = std::vector<std::future<void>>();
    threads_for_producers.reserve(producers.size());

    for (auto& prod : producers) {
        threads_for_producers.push_back(
                std::async(std::launch::async, &Producer<Data_type>::writing_to_queue_until, &prod,
                           std::ref(queue), std::ref(should_stop)));
    }

    auto threads_for_consumers = std::vector<std::future<void>>();
    threads_for_consumers.reserve(consumers.size());

    for (auto& cons : consumers) {
        threads_for_consumers.push_back(
                std::async(std::launch::async, &Consumer<Data_type>::reading_from_queue_until, &cons,
                           std::ref(queue), std::ref(should_stop)));
    }

    cfg.delay_func();
    should_stop.store(true, std::memory_order_relaxed);

    for (auto& thr : threads_for_producers) {
        thr.get();
    }

    for (auto& thr : threads_for_consumers) {
        thr.get();
    }

    auto all_prods_written_data = Data_type_vec{};
    for (auto& prod : producers) {
        auto data = prod.get_written_data();
        all_prods_written_data.insert(all_prods_written_data.end(), data.begin(), data.end());
    }

    auto all_cons_read_data = Data_type_vec{};
    for (auto& cons : consumers) {
        auto data = cons.get_read_data();
        all_cons_read_data.insert(all_cons_read_data.end(), data.begin(), data.end());
    }

    auto unread_data = queue.get_data();
    all_cons_read_data.insert(all_cons_read_data.end(), unread_data.begin(), unread_data.end());

    std::sort(all_prods_written_data.begin(), all_prods_written_data.end());
    std::sort(all_cons_read_data.begin(), all_cons_read_data.end());

    auto run_result = all_prods_written_data == all_cons_read_data;
    if (!run_result) {
        std::cout << '\"' << cfg.name << "\" failed\n";
    } else {
        std::cout << '\"' << cfg.name << "\" passed\n";
    }
    return run_result;
}
