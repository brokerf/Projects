#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include <time.h>
#include <unistd.h>
#include <string.h>

#include "game_structure.h"
#include "../tui/tui.h"
#include "vec.h"

int pass_time = 0;

void mine_lay(Field* field){

	/* the one time mine lay function, uses modulo for the probability of the field
	 * set in the setting, default at 15% */
	srand(time(NULL));
	for(int i = 0; i < vec_length(field->cells); i++){
		if(rand() % (100/field->settings.probability) == 0){
			Zelle* cell = *vec_at(field->cells,i);
			cell->mine = true;
			field->mines++;
		}
	}
}

void create_cells(Field* field){
	
	/* a one time function which creates the standard cell i.e. an empty, closed cell without a flag, 
	 * which is represented with a "." */
	
	Vec* vec = vec_new();
	for(int i = 1; i < field->settings.height+1; i++){
		for(int d = 1; d < field->settings.width+1; d++){
			Zelle* new = malloc(sizeof(Zelle));
			new->pos.x = d;
			new->pos.y = i;
			new->number = 0;
			new->mine = false;
			new->flag = false;
			new->deck = false;
			vec_push(vec,new);
			
		}
	}
	field->cells = vec;
}

Field* field_new(){
	Field* field = malloc(sizeof(Field));
	field->cursor_pos.x = 1; field->cursor_pos.y = 1;
	field->points = 0;
	field->time = 0;
	field->mines= 0;
	field->settings.width = 15; field->settings.height = 15;
	field->settings.probability = 15;
	field->lost = false;
	FILE *fptr = fopen("settings.txt", "r");
	if(fptr == 0){
		fptr = fopen("settings.txt", "w+");
		fprintf(fptr, "%d\n", field->settings.width);
		fprintf(fptr, "%d\n", field->settings.height);
		fprintf(fptr, "%d\n", field->settings.probability);
	}
	fclose(fptr);
	return field;
}

Field* read_settings(Field* field){
	int num;
	FILE* fptr = fopen("settings.txt", "r");
	if(fptr == NULL){
		return field;
	}
	fscanf(fptr, "%d", &num);
	field->settings.width = num;
	fscanf(fptr, "%d", &num);
	field->settings.height = num;
	fscanf(fptr, "%d", &num);
	field->settings.probability = num;
	fclose(fptr);
	return field;
}

void field_free(Field* field){
	free(field);
}

Matrix* draw_field(Field* field, Matrix* matrix){

	/* draw the playing field
	 * obviously its drawn more than it should be */

	Cell def  = (Cell){.content = ' ', .text_color = FG_BLACK, .background_color = BG_BLACK};
	Matrix* board = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	for(int i = 1; i < field->settings.height+1; i++){
		for(int d = 1; d < field->settings.width+1; d++){
			Cell* cell = matrix_cell_at(board, d, i);
			cell->content = '.';
			cell->background_color = BG_BLACK;
			cell->text_color = FG_WHITE;
		}
		Cell* cell = matrix_cell_at(board,field->settings.width+1, i);
		cell->content = ' ';
		cell->text_color = FG_WHITE;
		cell->background_color = BG_WHITE;
	}
	for(int p = 0; p < field->settings.width+2; p++){
		Cell* cell = matrix_cell_at(board, p, 0);
		cell->content = ' ';
		cell->text_color = FG_WHITE;
		cell->background_color = BG_WHITE;
		Cell* cell2 = matrix_cell_at(board, p, field->settings.height);
		cell2->content = ' ';
		cell2->text_color = FG_WHITE;
		cell2->background_color = BG_WHITE;
	}
	for(int e = 1; e< field->settings.height+1; e++){
		Cell* cell = matrix_cell_at(board, 0, e);
		cell->content = ' ';
		cell->text_color = FG_WHITE;
		cell->background_color = BG_WHITE;
	}
	Cell* cell = matrix_cell_at(board, 1, 1);
	cell->background_color = BG_WHITE;
	cell->text_color = FG_BLACK;
	matrix_set_str_at(board, 5, field->settings.height+2, "TIME:", FG_YELLOW, BG_BLACK);
	matrix_set_str_at(board, 2, field->settings.height+4, "SHORTCUTS:", FG_YELLOW, BG_BLACK);
	matrix_set_str_at(board, 4, field->settings.height+5, "W, A, S, D: MOVEMENT", FG_WHITE, BG_BLACK);
	matrix_set_str_at(board, 4, field->settings.height+6, "F: SET/REMOVE FLAG", FG_WHITE, BG_BLACK);
	matrix_set_str_at(board, 4, field->settings.height+7, "SPACE: REVEAL CELL", FG_WHITE, BG_BLACK);
	return board;
}



	
Matrix* movement(Field* field, Matrix* matrix, char c){
	Cell def = (Cell){.content = ' ', .background_color = BG_BLACK, .text_color = FG_BLACK};
	Matrix* board = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_print_update(board, matrix);
	Cell* cell = matrix_cell_at(board, field->cursor_pos.x, field->cursor_pos.y);
	if(c == 'w'){
		if(field->cursor_pos.y -1 == 0){
			return board;
		}
		else{
			
			cell->background_color = BG_BLACK;
			if(cell->text_color == FG_BLACK){
				cell->text_color = FG_WHITE;
			}	
			cell = matrix_cell_at(board, field->cursor_pos.x, field->cursor_pos.y -1);
			cell->background_color = BG_WHITE;
			if(cell->text_color == FG_WHITE){
				cell->text_color = FG_BLACK;
			}
			field->cursor_pos.y--;
		}
	}else if(c == 's'){
		if(field->cursor_pos.y + 1 == field->settings.height){
			return board;
		}else{
			
			cell->background_color = BG_BLACK;
			if(cell->text_color == FG_BLACK){
				cell->text_color = FG_WHITE;
			}
			cell = matrix_cell_at(board, field->cursor_pos.x, field->cursor_pos.y+1);
			if(cell->text_color == FG_WHITE){
				cell->text_color = FG_BLACK;
			}
			cell->background_color = BG_WHITE;
			field->cursor_pos.y++;
		}
	}else if(c == 'd'){
		if(field->cursor_pos.x +1 == field->settings.width+1){
			return board;
		}else{
			
			cell->background_color = BG_BLACK;
			if(cell->text_color == FG_BLACK){
				cell->text_color = FG_WHITE;
			}
			cell = matrix_cell_at(board, field->cursor_pos.x+1, field->cursor_pos.y);
			if(cell->text_color == FG_WHITE){
				cell->text_color = FG_BLACK;
			}
			cell->background_color = BG_WHITE;
			field->cursor_pos.x++;
		}
	}
	else if(c == 'a'){
		if(field->cursor_pos.x-1 == 0){
			return board;
		}else{
			
			cell->background_color = BG_BLACK;
			if(cell->text_color == FG_BLACK){
				cell->text_color = FG_WHITE;
			}
			cell = matrix_cell_at(board, field->cursor_pos.x-1, field->cursor_pos.y);
			cell->background_color = BG_WHITE;
			if(cell->text_color == FG_WHITE){
				cell->text_color = FG_BLACK;
			}
			field->cursor_pos.x--;
		}
	}
	return board;
}

Zelle* zelle_at(Field* field, int x, int y){

	/* holy shit, this actually works 
	 * for every width and height */

	int dist = field->settings.height - field->settings.width;	
	Zelle* zell = *vec_at(field->cells, (x-1) + (y-1)*(field->settings.height)-(dist*(y-1)));
	return zell;
}

void flag_set(Field* field, Matrix* matrix){
	Cell def = (Cell){.content = ' ', .background_color = BG_BLACK, .text_color = FG_BLACK};
	Matrix* update = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_print_update(update, matrix);
	Zelle* zell = zelle_at(field, field->cursor_pos.x, field->cursor_pos.y);
	Cell* cell = matrix_cell_at(update, field->cursor_pos.x, field->cursor_pos.y);
	if(!(zell->flag)){
		cell->content = 'X';
		cell->text_color = FG_BLUE;
		zell->flag = true;
		zell->deck = true;
		if(zell->mine){
			field->mines--;
		}
		
	}else{
		cell->content = '.';
		zell->deck = false;
		cell->text_color = FG_BLACK;
		zell->flag = false;
		if(zell->mine){
			field->mines++;
		}
	}
	matrix_print_update(matrix, update);
	matrix_free(update);
}

char int_convert(int x){
	char c = '0' + x;
	return c;
}

void time_update(Matrix* matrix, Field* field){
	Cell def = (Cell){.text_color = FG_BLACK, .content = ' ', .background_color = BG_BLACK};
	Matrix* update = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_print_update(update, matrix);
	time_t now = time(NULL);
	int dist = now - field->time;
	char* c = "TIME: ";
	char convert[1000];
	sprintf(convert, "%d", dist);
	strcat(c, convert);
	matrix_set_str_at(update, 5, field->settings.height+2, c, FG_YELLOW, BG_BLACK);
	matrix_print_update(matrix, update);
	matrix_free(update);
}

void count_number(Field* field, Zelle* zelle, int x, int y){
	int counter = 0; /* counter for the mines */
	int width = -1; /* starting points */
	int height = -1;
	int limit_width = 0;  /*limitations in the right direction */
	int limit_height = 0;

	if(x == 1){
		width = 0;
	}else if(x == field->settings.width -1){
		limit_width = 1;
			
	}
	if(y == 1){
		height = 0;
	}
	else if(y == field->settings.height-1){
		limit_height = 1;
	}

	for(int i = height; i < 2-limit_height; i++){
		for(int  d = width; d < 2-limit_width; d++){
			Zelle* cell = zelle_at(field, x + d, y + i);
			if(cell->mine){
				counter++;
			}
		}
	}
	zelle->number = counter;
}
	




Matrix* draw_cell(Matrix* matrix, Field* field, Zelle* zelle){
	Cell def = (Cell){.text_color = FG_BLACK, .background_color = BG_BLACK, .content = ' '};
	Matrix* new = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_print_update(new, matrix);
	Cell* cell = matrix_cell_at(new, zelle->pos.x, zelle->pos.y);
	if(!(zelle->deck)){
		if(!(zelle->flag)){
			if(zelle->mine){
				cell->content = '*';
				cell->text_color = FG_RED;
				matrix_print_update(matrix,new);
				field->lost = true;
				
			}
			else{
				count_number(field, zelle, zelle->pos.x, zelle->pos.y);
				if(zelle->number > 0){
					cell->content = int_convert(zelle->number);
					if(cell->background_color == BG_WHITE){
						cell->text_color = FG_BLACK;
					}else{
						cell->text_color = FG_WHITE;
					}
					matrix_print_update(matrix, new);
				}
				else{
					zelle->deck = true;
					for(int i = -1; i < 2; i++){
					       for(int d = -1; d < 2; d++){
						       if(zelle->pos.x + d  == 0){
							       d = 0;
						       }else if(zelle->pos.x + d == field->settings.width+1){
							       break;
						       }
						       if(zelle->pos.y + i == 0){
							       i = 0;
						       }else if(zelle->pos.y + i == field->settings.height){
							       break;
						       }
						       Zelle* zelle2 = zelle_at(field, zelle->pos.x + d, zelle->pos.y +i);
						       matrix_print_update(new, draw_cell(new, field, zelle2));
						       matrix_print_update(matrix, new);
					       }
					}
					cell->content = ' ';
					cell->text_color = BG_BLACK;
				}
			
			}
		}
	}
	zelle->deck = true;
	matrix_print_update(matrix, new);
	matrix_free(new);
	return matrix;
}	      


	
void game_over_screen(Matrix* matrix, Field* field){
	Cell def = (Cell){.content = ' ', .text_color = FG_BLACK, .background_color = BG_BLACK};
	Matrix* new = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_print_update(new, matrix);
	for(int i = 1; i < field->settings.height; i++){
		for(int d = 1; d < field->settings.width+1;d++){
			Zelle* zelle = zelle_at(field, d, i);
			if(zelle->flag){
				zelle->flag = false;
			}
			draw_cell(new, field, zelle);
		}
	}
	matrix_set_str_at(new, 2 ,field->settings.height+1, "YOU LOST X.X ", FG_HI_RED, BG_BLACK);
	matrix_print_update(matrix, new);
	field->lost = false;
	while(1){
		if(stdin_has_changed){
			char c = read_from_stdin();
			if(c == 'q'){
				break;
			}
		}
	}
	matrix_free(new);
}

void victory_screen(Matrix* matrix, Field* field){
	Cell def = (Cell){.content = ' ', .text_color = FG_BLACK, .background_color = BG_BLACK};
	Matrix* new = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	time_t now = time(NULL);
	int dist = now - field->time;
	field->points = field->settings.width * field->settings.height * field->settings.probability * (100/dist);
	matrix_print_update(new, matrix);
	FILE* fptr = fopen("new_highscore.txt", "r+");
	if(fptr == 0){
		fptr = fopen("new_highscore.txt", "w+");
	}
	for(int i = 1; i < field->settings.height; i++){
		for(int d = 1; d < field->settings.width+1; d++){
			Zelle* zelle = zelle_at(field, d, i);
			if(zelle->flag){
				zelle->flag = false;
			}
			draw_cell(new, field, zelle);
		}
	}
	matrix_set_str_at(new, 2, field->settings.height +1, "CONGRATULATIONS, YOU WON!", FG_GREEN, BG_BLACK);
	matrix_print_update(matrix, new);
	field->mines = 10;
	char a[] = "         ";
	sprintf(a,"%d", field->points);
	fprintf(fptr,"%d\n", field->points);
	*a = ' ';
	sprintf(a, "%d", dist);
	fprintf(fptr,"%d\n", dist);
	*a = ' ';
	sprintf(a, "%d", field->settings.width);
	fprintf(fptr,"%d\n", field->settings.width);
	*a = ' ';
	sprintf(a, "%d", field->settings.height);
	fprintf(fptr, "%d\n", field->settings.height);
	*a = ' ';
	sprintf(a, "%d", field->settings.probability);
	fprintf(fptr,"%d\n", field->settings.probability);
	*a = ' ';
	fclose(fptr);

	while(1){
		if(stdin_has_changed){
			char c = read_from_stdin();
			if(c == 'q'){
				break;
			}
		}
	}
}

void clean_cells(Field* field){
	for(int i = 0; i < vec_length(field->cells);i++){
		Zelle* zelle = *vec_at(field->cells, i);
		zelle->mine = false;
		zelle->deck = false;
		zelle->flag = false;
	}
}

void new_game(Matrix* matrix,Field* field){

	/* initializes a new game
	 * draw border and the field 
	 *which the size of is swet in settings */
	
	Cell def = (Cell){.content = ' ', .text_color = FG_BLACK, .background_color = BG_BLACK};
	field = read_settings(field);
	matrix_resize(matrix, 30 + field->settings.width, 10 + field->settings.height, &def);
	Matrix* board = draw_field(field,matrix);
	matrix_resize(board, 30 + field->settings.width, 10 + field->settings.height, &def);
	matrix_print_update(matrix, board);
	create_cells(field);
	pass_time = 0;
	field->cursor_pos.x = 1;
	field->cursor_pos.y = 1;
	field->time = time(NULL);
	mine_lay(field);
	while(1){
		if(pass_time == 100){
			Matrix* update = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
			matrix_print_update(update,matrix);
			time_t now = time(NULL);
			int dist = now - field->time;
			char str[1000000];
			sprintf(str, "%d", dist);
			matrix_set_str_at(update, 12, field->settings.height+2, str, FG_YELLOW, BG_BLACK);
			pass_time = 0;
			matrix_print_update(matrix, update);
			matrix_free(update);
		}
		if(stdin_has_changed()){
			char c = read_from_stdin();
			if(c == 'w' || c == 'a' || c == 's' || c == 'd'){
			       Matrix* board = movement(field, matrix, c);
			       matrix_print_update(matrix, board);
			}
	 		else if(c == 'q'){
				clean_cells(field);
				matrix_print_update(matrix, board);
				break;
			}
			else if(c == 'f'){
				flag_set(field, matrix);
				
			}else if(c == ' '){
				Zelle* zelle = zelle_at(field, field->cursor_pos.x , field->cursor_pos.y);
				matrix_print_update(matrix, draw_cell(matrix, field, zelle));
				
			}
			if(field->lost){
				game_over_screen(matrix, field);
				break;
			}
			else if(field->mines == 0){
				victory_screen(matrix, field);
				break;
			}
		}
		pass_time++;
	}
	field->lost = false;
	field->mines = 0;
	matrix_set_str_at(board, 2, field->settings.height +4, "                       ", FG_BLACK, BG_BLACK);
	matrix_set_str_at(board, 2, field->settings.height +5, "                       ", FG_BLACK, BG_BLACK);
	matrix_set_str_at(board, 2, field->settings.height +6, "                       ", FG_BLACK, BG_BLACK);
	matrix_set_str_at(board, 2, field->settings.height +7, "                       ", FG_BLACK, BG_BLACK);
	matrix_print_update(matrix, board);
	matrix_resize(matrix, 30, 30, &def);
	matrix_free(board);
}
