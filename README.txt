COPYRIGHT: Robert Gorden, 2021, robertgorden246@gmail.com

This is a supportive file, which should make some things more clearer as you play the game.
The width and height should be more around 20, 20. It can obviously go smaller, but if you go
for a bigger field, it might take more time to process, especially if you have a low probability
(i. e. around 1-10%). 

The probability is set as an integer, so if you want a probability of 15%, you set '15' in the settings menu.
The inner fields are processed differently. First the outer cells are drawn, i.e. the cells with mines around them,
then the inner fields are drawn. On smaller fields with a moderate probability, it will go unnoticed, but on a bigger field
especially with a very small probability, it will look bizarre as numbers are drawn randomly, and then afterwards all the inner fields
are drawn. This is normal. On the upside it has a nice, solitaire-like effect when all cells are revealed.

Settings and Highscores create each a .txt file, to read from it the corresponding content.

This file requires "vec.c" and "vec.h" to be in the same directory as this file and game,
while tui and its corresponding contents(ansi_codes.c/...h , tui_matrix.c/.h , tui_io.c/.h and tui.c/.h) can be in one folder 
named 'tui', so the path which the programm looks up should be "../tui/tui.c", otherwise it WILL NOT WORK!

P.S.: do NOT press 'e' in the start menu! 
