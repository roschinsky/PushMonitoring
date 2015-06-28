# PushMonitoring
A library and command line tool for sending notifications via push services about windows system states.

Push notifications are little alerts comming right to your phone, mobile client or desktop. They are fairly easy to send and won't spam your mailbox. So why not using them for a plain and simple notification service that reminds you of health issues of your IT environment? Sure you might say "What for? I use Nagios or MOM and redirect mails to eMail API of my push provider!" but what if you don't want to install and maintain a management infrastructure for your computers at home or a single server running mostly silently at home in the boxroom? 

That's exactly what PushMonitoring is meant for. Just create a scheduled task, fill in a XML file for configuration and you're ready to go! With one execution of _CMD_PushMonitoring_ you're able to execute multiple checks at once and trigger one or more push provider. With scheduled task you can easily let it run every time your client starts up or every hour - checks are executed and just if some of the defined checks are not resulting within the values you expect it'll give you a buzz. PushMonitoring supports Pushalot and Pushover services.

## Contents

### LIB_PushMonitoring

The library itself. Using a new *Monitoring* instance together with a XML configuration as string or file you already created your system monitor that handles checking and sending notifications for you. A *MonitoringConfig* class represents a config file and handles the XML configuration parsing. The classes *Notification*, *NotificationPushalot* and *NotificationPushover* are needed for creating and sending notifications to the push providers. *Check*, *CheckByState* and *CheckByValue* are the base classes for specific check types like *CheckDisk* or *CheckHttp*.

### CMD_PushMonitoring

A simple Windows command line application that'll use the library to run a monitoring and returns the result of the monitoring instance. You can set the location of the config file by command line parameter or the app.config.

## Current Version

The current version is working but a major feature for execution override by a defined amount of time is not implemented right now. This means that the application will only report the check results if any of them is not within the range of expected values. The feature i'll add for a "hey I'm still alive and everything is okay"...