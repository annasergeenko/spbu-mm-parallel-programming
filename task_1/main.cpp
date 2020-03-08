#include <vector>
#include <future>
#include <iostream>
#include <algorithm>

#include "queue_on_list.h"
#include "producer.h"
#include "consumer.h"

constexpr auto NUM_OF_PRODUCERS = 5;
constexpr auto NUM_OF_CONSUMERS = 5;
constexpr auto ELEM_AMOUNT      = 100;

using Data_type     = int;
using Data_type_vec = std::vector<Data_type>;

int main() {

    auto producers = std::vector<Producer<Data_type>>();
    producers.reserve(NUM_OF_PRODUCERS);

    for (Data_type i = 0; i < NUM_OF_PRODUCERS; ++i) {
        producers.emplace_back(Producer<Data_type>(
                Data_type_vec(ELEM_AMOUNT, i), std::chrono::seconds(i)));
    }

    auto consumers = std::vector<Consumer<Data_type>>();
    consumers.reserve(NUM_OF_CONSUMERS);

    for (Data_type i = 0; i < NUM_OF_CONSUMERS; ++i) {
        consumers.emplace_back(Consumer<Data_type>(std::chrono::seconds(i)));
    }

    auto queue = Queue_on_list<Data_type>();
    bool should_stop = false;
    auto threads_producers = std::vector<std::future<void>>();
    auto threads_consumers = std::vector<std::future<void>>();

    for (auto& prod : producers) {
        threads_producers.push_back(
                std::async(std::launch::async, &Producer<Data_type>::writing_to_queue_until, &prod,
                        std::ref(queue), std::ref(should_stop)));
    }

    for (auto& cons : consumers) {
        threads_consumers.push_back(
                std::async(std::launch::async, &Consumer<Data_type>::reading_from_queue_until, &cons,
                           std::ref(queue), std::ref(should_stop)));
    }

    std::cout << "Press Enter to stop threads...\n";
    std::string tmp{};
    getline(std::cin, tmp);
    should_stop = true;
    std::cout << "Waiting for threads...\n";

    for (auto& thr : threads_producers) {
        thr.get();
    }

    for (auto& thr : threads_consumers) {
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

    std::sort(all_prods_written_data.begin(), all_prods_written_data.end());
    std::sort(all_cons_read_data.begin(), all_cons_read_data.end());

    if (all_prods_written_data != all_cons_read_data) {
        std::cout << "Failed\n";
    } else {
        std::cout << "Correct\n";
    }
}