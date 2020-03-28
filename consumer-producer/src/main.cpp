#include <chrono>
#include <list>
#include <iostream>
#include <future>

#include "Utils.h"
#include "Buffer.h"
#include "Producer.h"
#include "Consumer.h"


int main() {
  Buffer<unsigned> Buf;
  volatile bool ShouldStop(false);

  std::cout << "Press <Enter> key after user input "
            << "to finish execution of the program.\n";

  // Input for Producers
  std::cout << "Enter the number of producers: ";
  unsigned ProducersNum;
  std::cin >> ProducersNum;
  std::list<Producer<unsigned>> ProducersList;
  for (auto i = 0; i < ProducersNum; ++i) {
    std::cout << "Enter the cooldown (in ms) for Producer #" << i << " and "
              << "number of tasks should be produced: ";
    unsigned Cooldown, NumTasks;
    std::cin >> Cooldown >> NumTasks;
    ProducersList.emplace_back(generateRandomIntData<unsigned>(NumTasks),
                               Buf, ShouldStop,
                               std::chrono::milliseconds(Cooldown));
  }

  // Input for Consumers
  std::cout << "Enter the number of consumers: ";
  unsigned ConsumersNum;
  std::cin >> ConsumersNum;
  std::list<Consumer<unsigned>> ConsumersList;
  for (auto i = 0; i < ConsumersNum; ++i) {
    std::cout << "Enter the cooldown (in ms) for Consumer #" << i << ": ";
    unsigned Cooldown;
    std::cin >> Cooldown;
    ConsumersList.emplace_back(
        Buf, ShouldStop,
        std::chrono::milliseconds(Cooldown));
  }

  // Start Consumers
  AsyncReturnType<unsigned> ConsumerResults;
  for (auto&& C: ConsumersList)
    ConsumerResults.push_back(std::async(
        std::launch::async,
        &Consumer<unsigned>::takeTasks,
        &C
        ));
  // Start Producers
  AsyncReturnType<unsigned> ProducerResults;
  for (auto&& P : ProducersList)
    ProducerResults.push_back(std::async(
        std::launch::async,
        &Producer<unsigned>::start,
        &P));

  // A bunch of code to just wait for <Enter> key from user
  std::cin.clear();
  std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
  std::cin.get();

  // Consumers and Producers are going to finish their work after that
  ShouldStop = true;

  return 0;
}
