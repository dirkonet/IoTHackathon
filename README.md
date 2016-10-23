# IoTHackathon
Our winning solution to the October 2016 Microsoft Hack@Home Hackathon in Dresden. As the technology partner was ThyssenKrupp Elevators, the task was to combine elevators and IoT technology for predictive maintenance.

## The team
Team qed∎ consisted of Daniel Linke, [Dirk Legler](https://github.com/dirkonet) and [Sascha Peukert](https://github.com/SaschaPeukert).

## The idea
ThyssenKrupp already uses data like elevator cable runtime to predict maintenance needs. We gather additional data from inside and outside the elevator to improve both customer experience and maintenance handling. For now, we collect temperature and humidity data, brightness and loudness. This allows the elevator to choose a certain floor with the right climate to ventilate during standby time or dim the elevator music if people are talking to each other. Additinally, events like a sudden increase in humidity inside the elevator can indicate a spill that needs to be cleaned.

## The code
The project consists of three parts: The measuring device (a Raspberry Pi 2 with GrovePi sensors running Windows 10 IoT), the cloud data handling (IoT Hub, Database,…) and a smartphone app for the maintenance personell (Xamarin based).
