#include "config_runner.h"

void sleep_for_seconds(int pause) {
    std::this_thread::sleep_for(std::chrono::seconds(pause));
}

using Config_vec = std::vector<Config>;

Config_vec generate_tests_configs() {
    auto res = Config_vec{};
    auto default_pause = std::chrono::seconds(1);

    auto cfg = Config{};
    cfg.name = "Not enough time to process";
    cfg.num_of_prods = 5;
    cfg.prod_elem_amount = 100;
    cfg.pause_for_producers = default_pause;
    cfg.num_of_cons = 5;
    cfg.pause_for_consumers = default_pause;
    cfg.delay_func = [](){
        sleep_for_seconds(1);
    };
    res.emplace_back(cfg);

    cfg = {"One producer, many consumers", 1, 5, default_pause, 10, default_pause, [](){
        sleep_for_seconds(6);
    }};
    res.emplace_back(cfg);

    cfg = {"Many producers, one consumer", 5, 1, default_pause, 1, default_pause, [](){
               sleep_for_seconds(6);
           }};
    res.emplace_back(cfg);

    return res;
}

int main() {
    auto test_cfgs = generate_tests_configs();
    bool all_tests_were_passed = std::all_of(test_cfgs.begin(), test_cfgs.end(), [](auto&& cfg){
        return run_config(cfg);
    });
    if (all_tests_were_passed) {
        std::cout << "All tests were passed\n";
    } else {
        std::cout << "Error(s) occurred\n";
    }
}
