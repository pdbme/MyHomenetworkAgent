# MyHomenetworkAgent
This agent written in C# enables cloud services to call API endpoints of your local api-enabled applications from inside your homenetwork.

Example: You locally operate sabnzbd and want to connect it to a cloud service like app.porndb.me but the cloud service cannot directly call your local sabnzbd because you do not want to make it public to the internet. Once installed on a machine inside your home network, the agent regularly polls the cloud service for new commands (e.g. make API-calls) and sends back the results from api calls made. In addition it is possible to search specific local directories on the machine the agent is installed on. This enables the cloud service to compare what you already downloaded versus what is missing and might be of interest for you. Of course access to your files is optional and can be completely disable in the local settings (so you are in full control).

Since this software is open source and pretty simple to review and 
