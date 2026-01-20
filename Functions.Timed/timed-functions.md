# scheduled Functions 

## Overview 

As I researched the topic of creating functions that run at regular intervals, most people recommended keeping your scheduled functions separate from your main application. This is because it gives you more possible ways to run your scheduled functions, such as Azure functions, cron jobs, or even simple console applications that you run using Windows Task Scheduler or Linux Cron. 

It also allows you to run your scheduled functions in a separate process or even on a separate machine. This can help prevent your main application from being affected by the scheduled function or in some other way keeping your main application busy with the workload of the scheduled function. 

It is also easier to now run it on a completetly different machine/server, meaning we can now for sure that the scheduled function isn't affecting the main application.

This folder is a very basic separation of main app and scheduled functions to visualize the concept. 