 _______  _______ _________ _        _______           _______
(  ____ \(  ___  )\__   __/( (    /|(  ____ \|\     /|(  ____ \
| (    \/| (   ) |   ) (   |  \  ( || (    \/| )   ( || (    \/
| |      | |   | |   | |   |   \ | || |      | (___) || (
| |      | |   | |   | |   | (\ \) || |      |  ___  ||  __)
| |      | |   | |   | |   | | \   || |      | (   ) || (
| (____/\| (___) |___) (___| )  \  || (____/\| )   ( || (____/\
(_______/(_______)\_______/|/    )_)(_______/|/     \|(_______/

Welcome to the Coinche project !

This coinche projects implements a playing card game called "Bataille",
which sounds pretty epic, right ?! In order to play the Bataille, you
may launch at least one server and two clients.

Want to launch a demo ? try \> launch.bat

In the coinche.sln solution you will find several projects:
  I)    Server            the Coinche server
  II)   Client            the Coinche client
  III)  Shared            client/server shared classes
  IV)   UnitTestCoinche   unit testing

=== I - Server ===

The server is so much epic that is protected by a password. Would you
be able to find it in the code by some reversing engineering magic ?!
No, of course because it is hashed in the code. The couple admin:admin1234
let's you open the server by hashing and serializing via md5 and base64
the creditentials.

When launched, the server provides an ip and a port where clients can now
connect and start to discuss. You can spy on them using the admin chatbox

=== II - Client ===

So you launched the client side ? Pretty nice splashscreen huh ?! :-)
Please connect to a server and start chatting with other players around
the world. When you are ready, click on "play with others" to start the
Bataille. You can still talk with your fellows but not only with them now,
you are in the Bataille jail. Win the game to escape or be trapped for ever !

=== III - Shared ===

Shared/ is code that is shared between Client/ and Server/ It contains routines
such as Card and Deck handling.

=== IV - UnitTestCoinche ===

What would happen if Judas and Jesus would battle within the Coinche ? What
would happen if Bouddha joined them ? UnitTestCoinche is answering such,
also is testing the project with such particular cases.
