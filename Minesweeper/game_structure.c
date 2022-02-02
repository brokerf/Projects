#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include "game_structure.h"
#include "game_lib.h"
#include "vec.h"


// use Int2 as the positional argument of the cell //
typedef struct int2 int2;
// cell has a position, which is used as position in the Vec, bool for mine and flag //
typedef struct Zelle Zelle;


// define the Field with min position, max position, cursor position and an array of all cells //
typedef struct Field Field;

bool cell_flag(Zelle* cell){
	return cell->flag;
}
bool cell_mine(Zelle* cell){
	return cell->mine;
}


void free_field(Field* field){
	vec_free(field->cells);
	free(field);
}
bool decke_auf(Zelle* cell){
	return cell->deck;
}


bool compare(int x, int y, int a, int b){
	if(x == a && y == b){
		return true;
	}
	return false;
} 


void indirekt_draw(Matrix* matrix, Field* field, Zelle* zelle, int x, int y){
	Cell def = (Cell){.content = ' ', .background_color = BG_BLACK, .text_color = FG_BLACK};
	Matrix* update = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_print_update(update, matrix);
	if(!(compare(zelle->pos.x, zelle->pos.y, x, y))){
		count_number(field, zelle, zelle->pos.x, zelle->pos.y);
		draw_cell(update, field, zelle);
		matrix_print_update(matrix, update);
		
	}
}



void indirekt_cell(Matrix* matrix, Field* field, Zelle* zelle){
	int width = -1;
	int height = -1;
	int width_limit = 0;
	int height_limit = 0;
	Cell def = (Cell){.content = ' ', .background_color = BG_BLACK, .text_color = FG_BLACK};
	Matrix* update = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	if(zelle->pos.x == 1){
		width = 0;
	}else if(zelle->pos.x == field->settings.width-1){
		width_limit = 1;
	}
	if(zelle->pos.y == 1){
		height = 0;
	}else if(zelle->pos.y == field->settings.height-1){
		height_limit = 1;
	}
	zelle->deck = true;
	for(int i = height; i < 2-height_limit; i++){
		for(int d = width; d < 2 - width_limit; d++){
			Zelle* zelle2 = zelle_at(field, zelle->pos.x + d, zelle->pos.y + i);
			count_number(field, zelle2, zelle2->pos.x, zelle2->pos.y);
			if(zelle2->number == 0){
				indirekt_cell(matrix, field, zelle2);
			}
			else{
				update = draw_cell(update, field, zelle2);
				matrix_print_update(matrix, update);
			}
		}
	}
	
}
		



