# PushMonitoring
A library and command line tool for sending notifications via push services about windows system states.

Push notifications are little alerts comming right to your phone, mobile client or desktop. They are fairly easy to send and won't spam your mailbox. So why not using them for a plain and simple notification service that reminds you of health issues of your IT environment? Sure you might say "What for? I use Nagios or MOM and redirect mails to eMail API of my push provider!" but what if you don't want to install and maintain a management infrastructure for your computers at home or a single server running mostly silently at home in the boxroom? 

![PushMonitoring executed](https://troschinsky.files.wordpress.com/2015/06/pushnotification_execution1.jpg?w=630)

That's exactly what PushMonitoring is meant for. Just create a scheduled task, fill in a XML file for configuration and you're ready to go! With one execution of _CMD_PushMonitoring_ you're able to execute multiple checks at once and trigger one or more push services. With scheduled tasks you can easily let it run every time your client starts up or every hour - checks are executed and just if some of the defined checks are not resulting within the values you expect it'll give you a buzz. 

![PushMonitoring received in mobile client](https://troschinsky.files.wordpress.com/2015/06/pushnotification_messagereceived1.jpg?w=630)

PushMonitoring supports Pushalot, Pushover and Telegram services.

## Contents

### LIB_PushMonitoring

The library itself. Using a new *Monitoring* instance together with a XML configuration as string or file you already created your system monitor that handles checking and sending notifications for you. A *MonitoringConfig* class represents a config file and handles the XML configuration parsing. The classes *Notification*, *NotificationPushalot* and *NotificationPushover* are needed for creating and sending notifications to the push providers. *Check*, *CheckByState* and *CheckByValue* are the base classes for specific check types like *CheckDisk* or *CheckHttp*.

### CMD_PushMonitoring

A simple Windows command line application that'll use the library to run a monitoring and returns the result of the monitoring instance. The most important thing is the config file. It holds all the checks, that you'd like to execute with each run.

![PushMonitoring config](https://troschinsky.files.wordpress.com/2015/06/pushnotification_config1.jpg?w=630)

You can set the location of the config file by command line parameter or the app.config.

## Current Version

The current version is working but some features are still missing. Recently the feature for execution-override by a defined amount of time was implemented. That means that the application, while normal execution, will only report the check results if any of them is not within the range of expected values. But once the override interval is reached, the applications sends the notification including current report, even if there was not a problem detected. The feature might come in handy when you want the application to say "hey I'm still alive and everything is okay"...
