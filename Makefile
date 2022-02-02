CFLAGS= -fsanitize=address -g -Wall

.PHONY: clean


clean:
	rm -f *.o game ../tui/*.o new_highscore.txt

checkstyle:
	clang-tidy --quiet $(wildcard *.c) $(wildcard *.h) --

game: game.o game_structure.o game_lib.o vec.o ../tui/tui.o ../tui/tui_io.o ../tui/tui_matrix.o ../tui/ansi_codes.o
	gcc game.o game_structure.o game_lib.o vec.o ../tui/tui.o ../tui/tui_io.o ../tui/tui_matrix.o ../tui/ansi_codes.o -o game
	./game
	rm ./game


game.o: 
	gcc -c game.c -o game.o

vec.o:
	gcc -c vec.c -o vec.o

game_lib.o:
	gcc -c game_lib.c -o game_lib.o

game_structure.o:
	gcc -c game_structure.c -o game_structure.o

../tui/tui.o:
	gcc -c ../tui/tui.c -o ../tui/tui.o

../tui/tui_io.o:
	gcc -c ../tui/tui_io.c -o ../tui/tui_io.o

../tui/tui_matrix.o:
	gcc -c ../tui/tui_matrix.c -o ../tui/tui_matrix.o

../tui/ansi_codes.o:
	gcc -c ../tui/ansi_codes.c -o ../tui/ansi_codes.o


