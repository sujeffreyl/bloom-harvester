The settings.ini file in this folder is for RestartOnCrash.
RestartOnCrash can be downloaded from here: https://w-shadow.com/blog/2009/03/04/restart-on-crash/
The settings.ini file should be copied to the same folder as the RestartOnCrash application.

Here is how we currently keep Harvester running.
Harvester itself runs in a loop, checking periodically if there are new books to process
RestartOnCrash checks Harvester almost constantly, making sure the Harvester process hasn't crashed or hung.
Windows Task Scheduler starts up RestartOnCrash when the user logs in to the computer.


Additional Remarks:
If you change the settings via the RestartOnCrash UI, the changes should propagate through immediately. However, if you modify the settings.ini file outside of the RestartOnCrash application, then you need to restart the RestartOnCrash application so that it would re-load the new settings.

If you change the environment variables, you need to restart the application to see the new values.