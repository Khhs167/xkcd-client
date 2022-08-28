# XKCD Client
![xkcd #1217](images/2664.png)

This is just a stupid little project i decided to make for fun. The code is basic, poorly written, but does work both online and offline!  

To use it, just do `dotnet run` or build it and run the executable. 

If online it'll download the latest comic, if offline, it'll display the latest downloaded comic. If offline and no comics are stored, it will shut down.  

Once inside the program, just click to load another random comic, either online or offline.

Command line args:  
Feel free to pass in the ID of a comic as an argument(`./xkcd 2664` for example), or you can do `./xkcd download` to begin downloading all comics!  

Special comic:  
There is a special comic, the 404 comic, which i added manually. xkcd doesnt itself have a comic n.o 404, so instead this repo contains a makeshift one, alongside code to fetch that one instead!  
This comic will show if the comic is number 404, as an easter egg, or if loading failed(jpeg comics for example, see [xkcd 1](https://xkcd.com/1))