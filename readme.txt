Please close MotherBuilder and ChildBuilderProc consoles before starting another instance of mother builder from GUI. 

I can close the console by killing the process but I wanted to show that all the requirements are satisfied. So I am not
closing the consoles manually. 

Please don't try to build any request or send any message from any component using mother builder and child builders started using GUI. 
Doing this will lead to exception as earlier mother builder is having one fixed binding registered and after starting new process that 
binding will change even though both processes will listen to same port and repository will treat that new process as unregistered 
process because of different binding. 

If TAs want to check quit functionality of the system then please click on Stop Mother Builder button in GUI before starting new mother builder 
and child builders. TAs can click on Stop Mother Builder button for quiting mother and children running after starting the project.
