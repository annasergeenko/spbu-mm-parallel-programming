#include "config_runner.h"

constexpr int NUM_OF_PRODUCERS = 5;
constexpr int NUM_OF_CONSUMERS = 5;
constexpr int ELEM_AMOUNT      = 100;


void wait_for_enter_and_return() {
    std::cout << "Press Enter to stop work...\n";
    std::string tmp;
    getline(std::cin, tmp);
    std::cout << "Waiting for threads...\n";
}


int main() {
    auto cfg = Config{};
    cfg.name = "Run until enter pressed";
    cfg.num_of_prods = NUM_OF_PRODUCERS;
    cfg.prod_elem_amount = ELEM_AMOUNT;
    cfg.pause_for_producers = static_cast<std::chrono::seconds>(1);
    cfg.num_of_cons = NUM_OF_CONSUMERS;
    cfg.pause_for_consumers = static_cast<std::chrono::seconds>(1);
    cfg.delay_func = wait_for_enter_and_return;
    run_config(cfg);
}