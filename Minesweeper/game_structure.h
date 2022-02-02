#ifndef GAME_STRUCTURE_H
#define GAME_STRUCTURE_H

#include <stddef.h>
#include <stdbool.h>
#include "vec.h"
#include "../tui/tui_matrix.h"
#include "../tui/tui.h"
typedef struct int2{
 	int x;
  	int y;	
}int2;

typedef struct Zelle{
	int2 pos;
	int number;
	bool mine;
	bool flag;
	bool deck;
}Zelle;

typedef struct Field{
	int2 cursor_pos;
	Vec* cells;
	int points;
	int mines;
	time_t time;
	bool lost;
	struct settings{
		int width;
		int height;
		int probability;
	}settings;
}Field;

bool cell_flag(Zelle* cell);

bool cell_mine(Zelle* cell);

void free_field(Field* field);

bool decke_auf(Zelle* cell);

void indirekt_cell(Matrix* matrix, Field* field,Zelle* zelle);
#endif
