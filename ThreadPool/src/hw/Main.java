package hw;

import java.util.LinkedList;
import java.util.List;
import java.util.Optional;
import java.util.Scanner;

// command line API for hw.threadpool.FixedThreadPool
public class Main {
    public static void main(String[] args) {
        int nThreads = 2;
        new Main().start(nThreads);
    }
    private static void usage() {
        System.out.println("Usage:");
        System.out.println("new <duration_secs>         new task with specified duration in seconds");
        System.out.println("stop <task_id>              stop the task");
        System.out.println("status <task_id>            status of the task");
        System.out.println("dispose                     no new tasks");
    }

    private void start(int nThreads) {
        Scanner in = new Scanner(System.in);
        ThreadPool pool = new FixedThreadPool(nThreads);
        List<iMyTask> tasks = new LinkedList<>();
        usage();
        while (true) {
            Command cmd = new Command(in.nextLine());
            switch (cmd.getType()) {
                case "new":
                    if (cmd.hasIntArg() && cmd.getIntArg() >= 0) {
                        int duration_secs = cmd.getIntArg();
                        int taskId = tasks.size();
                        iMyTask task = new iMyTask(() -> {
                            System.out.printf("Starting task id=%d for %d\n", taskId, duration_secs);
                            Thread.sleep(duration_secs * 1000);
                            System.out.printf("DONE task id=%d\n", taskId);

                            return Optional.empty();
                        });

                        tasks.add(task);
                        pool.submit(task);
                        continue;
                    }
                case "stop":
                    if (cmd.hasIntArg()) {
                        int index = cmd.getIntArg();
                        if (0 < index && index <= tasks.size()) {
                            tasks.get(cmd.getIntArg()).cancel();
                        } else {
                            System.out.println("no such task");
                        }
                        continue;
                    }
                case "status":
                    if (cmd.hasIntArg()) {
                        int index = cmd.getIntArg();
                        if (0 < index && index <= tasks.size()) {
                            System.out.println(tasks.get(cmd.getIntArg()).Result());
                        } else {
                            System.out.println("no such task");
                        }
                        continue;
                    }
                    case "dispose":
                        try {
                            pool.awaitCompletion();
                        } catch (InterruptedException e) {
                            e.printStackTrace();
                        }
                        continue;
            }
            usage();
        }
    }

    private static class Command {
        private String type = "";
        private boolean hasIntArg;
        private int intArg;

        Command(String input) {
            String[] parts = input.split(" ");
            if (parts.length > 0) {
                type = parts[0].toLowerCase();
            }
            if (parts.length == 2) {
                try {
                    intArg = Integer.parseInt(parts[1]);
                    hasIntArg = true;
                } catch (NumberFormatException e) {
                }
            }
        }

        String getType() {
            return type;
        }

        boolean hasIntArg() {
            return hasIntArg;
        }

        int getIntArg() {
            return intArg;
        }
    }
}