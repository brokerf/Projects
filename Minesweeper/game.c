#include <stdio.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>

#include "../tui/tui.h"
#include "game_lib.h"
#include "vec.h"


int point = 2;

Matrix* move_down(Matrix* matrix,char c){
	
	/*general movement, despite the name
	 * looks for c and decides what it should do to point and point2
	 * where point2 was previous pos of point */

	int point2;
	if(c == 'w'){
		point--;
		point2 = point+1;

	}else if(c == 's'){
		point++;
		point2 = point-1;
	}
	Cell def = (Cell){.content = ' ', .background_color = BG_BLACK, .text_color = FG_WHITE};
	Matrix* new = matrix_new(30,30, &def);
	matrix_print_update(new, matrix);
	if(point == 6){
		point = 5;
		return new;
	}
	if(point == 1){
		point = 2;
		return new;
	}
	char line[30] = "";
	char line2[30] = "";
	for(int i = 0; i < matrix_width(matrix); i++){
		Cell* cell = matrix_cell_at(matrix, i, point);
		Cell* cell2 = matrix_cell_at(matrix, i, point2);
		line[i] = cell->content;
		line2[i] = cell2->content;
	}
	matrix_set_str_at(new, 0, point2, line2, FG_WHITE, BG_BLACK);
	matrix_set_str_at(new, 0, point, line, FG_YELLOW, BG_BLACK);
	return new;
	
}

Matrix* move_setting(Matrix* matrix, char c){
	int point2;
	if(c == 'w'){
		point--;
		point2 = point+1;
	}else if(c == 's'){
		point++;
		point2 = point-1;
	}
	Cell def = (Cell){.content = ' ',. background_color = BG_BLACK, .text_color = FG_WHITE};
	Matrix* new = matrix_new(30,30,&def);
	matrix_print_update(new, matrix);
	if(point == 5){
		point = 4;
		return new;
	}
	if(point == 1){
		point = 2;
		return new;
	}
	char line[30] = "";
	char line2[30] = "";
	for(int i = 0; i < matrix_width(matrix); i++){
		Cell* cell = matrix_cell_at(matrix, i, point);
		Cell* cell2 = matrix_cell_at(matrix, i, point2);
		line[i] = cell->content;
		line2[i] = cell2->content;
	}
	matrix_set_str_at(new, 0, point2, line2, FG_WHITE, BG_BLACK);
	matrix_set_str_at(new, 0, point, line, FG_YELLOW, BG_BLACK);
	return new;
}

void reduce(Matrix* update, char* text, Field* field){
	switch(point){
		case 2: 
			field->settings.width--;
			sprintf(text, "%d", field->settings.width);
			break;

		case 3: 
			field->settings.height--;
			sprintf(text, "%d", field->settings.height);
			break;
		
		case 4: 
			field->settings.probability--;
			sprintf(text, "%d", field->settings.probability);
			break;
	}
	matrix_set_str_at(update, 15, point, "  ", FG_YELLOW, BG_BLACK);
	matrix_set_str_at(update, 15, point,  text, FG_YELLOW, BG_BLACK);
}

void increase(Matrix* update, char* text, Field* field){
	switch(point){
		case 2: 
			field->settings.width++;
			sprintf(text, "%d", field->settings.width);
			break;
			
		
		case 3: 
			field->settings.height++;
			sprintf(text, "%d", field->settings.height);
			break;
		
		case 4: 
			field->settings.probability++;
			sprintf(text, "%d", field->settings.probability);
			break;
	}
	matrix_set_str_at(update, 15, point, "  ", FG_YELLOW, BG_BLACK);
	matrix_set_str_at(update, 15, point, text, FG_YELLOW, BG_BLACK);
}




Field* action_settings(Matrix* matrix,Field* field){

	/* more complicated, should return a field for the game, with
	 * new settings, for width or height or probability */

	Cell def = (Cell){.content = ' ', .text_color = FG_WHITE, .background_color = BG_BLACK};
	Matrix* update = matrix_new(matrix_width(matrix),matrix_height(matrix),&def);
	matrix_print_update(update, matrix);
	while(1){
		if(stdin_has_changed()){
			char c = read_from_stdin();
			if(c == ' ' || c == 'q'){
				break;
			}
			char text[] = "   ";
			if(c == 'a'){
				reduce(update,text,field);
				matrix_print_update(matrix, update);
				
			}else if(c == 'd'){
				increase(update, text,field);
				matrix_print_update(matrix, update);
				
			}
		}
	}
	return field;

}

void setting(Matrix* matrix,Field* field){

	/* Menu for the settings, with width, height and probability*/
	/* adds general movement for both ways */
	
	FILE *ptr = fopen("settings.txt", "r");

	if(ptr != NULL){
		int* new_width = malloc(2*sizeof(int));
		int* new_height = malloc(2* sizeof(int));
		int* new_probability = malloc(2*sizeof(int));
		fscanf(ptr, "%d", new_width);
		fscanf(ptr, "%d", new_height);
		fscanf(ptr, "%d", new_probability);
		field->settings.width = *new_width;
		field->settings.height = *new_height;
		field->settings.probability = *new_probability;
		free(new_width);
		free(new_height);
		free(new_probability);
	}
	fclose(ptr);
	point = 2;
	Cell def = (Cell){.content = ' ', .text_color = FG_BLACK, .background_color = FG_BLACK}; 
	Matrix* new = matrix_new(30, 30, &def);
	char text[3] = "  ";
	char width[25] = "WIDTH        ";
	char height[25] ="HEIGHT       ";
	char prob[25] =  "PROBABILITY  ";
	sprintf(text, "%d", field->settings.height);
	strcat(height, text);
	
	sprintf(text, "%d", field->settings.width);
	strcat(width, text);
	
	sprintf(text, "%d", field->settings.probability);
	strcat(prob, text);
	matrix_set_str_at(new, 1, 1, "SETTINGS:", FG_RED, BG_BLACK);
	matrix_set_str_at(new, 2, 2, width, FG_YELLOW, BG_BLACK);
	matrix_set_str_at(new, 2, 3, height, FG_WHITE, BG_BLACK);
	matrix_set_str_at(new, 2, 4, prob, FG_WHITE, BG_BLACK);

	matrix_set_str_at(new, 1, 6, "SHORTCUTS:", FG_RED, BG_BLACK);
	matrix_set_str_at(new, 2, 7, "W/S: MOVE UP/MOVE DOWN", FG_WHITE, BG_BLACK);
	matrix_set_str_at(new, 2, 8, "A/D: DECREASE/INCREASE ITEM", FG_WHITE, BG_BLACK);
	matrix_set_str_at(new, 2, 9, "SPACE: CHOOSE ITEM TO EDIT", FG_WHITE, BG_BLACK);
	matrix_set_str_at(new, 2, 10, "Q: RETURN TO MENU", FG_WHITE, BG_BLACK);
	matrix_set_str_at(new, 2, 11, "PRESS SPACE TO UNSELECT ", FG_WHITE, BG_BLACK);
	matrix_set_str_at(new, 2, 12, "THE ITEM", FG_WHITE, BG_BLACK);
	matrix_print_update(matrix, new);
	while(1){
		if(stdin_has_changed()){
			char c = read_from_stdin();
			if (c == 's'|| c == 'w' ){	
				new = move_setting(matrix,c);
				matrix_print_update(matrix, new);
				free(new);
			}else if(c == ' '){
				action_settings(matrix,field);
			}else if(c == 'q'){
				break;
			}
		
		}
	
	}
}


Matrix* highscore_menu(Matrix* matrix){

	/*draw the highscore menu with all the Information needed*/
	
	Cell def = (Cell){.content = ' ', .text_color = FG_BLACK, .background_color = BG_BLACK};
	Matrix* update = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_set_str_at(update, 1, 2, "TOP 10 HIGHSCORES", FG_RED, BG_BLACK);
	matrix_set_str_at(update, 3, 4, "POINTS   TIME   WIDTH   HEIGHT   PROBABILITY", FG_WHITE, BG_BLACK);
	return update;
}




Matrix* draw_highscores(Matrix* matrix){
	Cell def = (Cell){.text_color = FG_BLACK, .background_color = BG_BLACK, .content = ' '};
	Matrix* highscore = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_print_update(highscore, matrix);
	FILE* fptr = fopen("highscores.txt", "w+");
	FILE* edit = fopen("edit.txt", "r");
	int edit_point;
	int edit_time;
	int edit_width;
	int edit_height;
	int edit_prob;
	
	for(int i = 0; i < 10; i++){
		fscanf(edit, "%d", &edit_point);
		fprintf(fptr, "%d ", edit_point);
		fscanf(edit, "%d", &edit_time);
		fprintf(fptr, "%d ", edit_time);
		fscanf(edit, "%d", &edit_width);
		fprintf(fptr, "%d ", edit_width);
		fscanf(edit, "%d", &edit_height);
		fprintf(fptr, "%d ", edit_height);
		fscanf(edit, "%d", &edit_prob);
		fprintf(fptr, "%d\n", edit_prob);
	}
	fclose(fptr);
	fptr = fopen("highscores.txt", "r");
	for(int i = 0; i < 10; i++){
		char a[] = "          ";
		fscanf(fptr, "%d", &edit_point);
		if(edit_point > 0){
			fscanf(fptr, "%d", &edit_time);
			fscanf(fptr, "%d", &edit_width);
			fscanf(fptr, "%d", &edit_height);
			fscanf(fptr, "%d", &edit_prob);
			sprintf(a, "%d", edit_point);
			matrix_set_str_at(highscore, 3, 6 + i, a, FG_WHITE, BG_BLACK);
			strcpy(a, "        ");
			sprintf(a, "%d", edit_time);
			matrix_set_str_at(highscore, 12, 6 + i, a, FG_WHITE, BG_BLACK);
			strcpy(a, "        ");
			sprintf(a, "%d", edit_width);
			matrix_set_str_at(highscore, 19, 6 + i, a, FG_WHITE, BG_BLACK);
			strcpy(a, "        ");
			sprintf(a, "%d", edit_height);
			matrix_set_str_at(highscore, 27, 6 + i, a, FG_WHITE, BG_BLACK);
			strcpy(a, "        ");
			sprintf(a, "%d", edit_prob);
			matrix_set_str_at(highscore, 36, 6 + i, a, FG_WHITE, BG_BLACK);
		}
	}

	fclose(fptr);
	fclose(edit);
	
	remove("edit.txt");
	return highscore;
}





void highscores(Matrix* matrix, Field* field){
	Cell def = (Cell){.content = ' ', .text_color = FG_BLACK, .background_color = BG_BLACK};
	matrix_resize(matrix, 100, 100, &def);
	Matrix* new = highscore_menu(matrix);
	matrix_print_update(matrix, new);
	matrix_free(new);
	FILE* fptr = fopen("highscores.txt", "r+");
	if(fptr == NULL){
		fptr = fopen("highscores.txt", "w");
		fprintf(fptr, "0 0 0 0 0");
		fclose(fptr);
		fptr = fopen("highscores.txt", "r+");
	}
	FILE* edit = fopen("edit.txt", "w+");
	FILE* new_fptr = fopen("new_highscore.txt", "r+");
	int new_points = 20;
	int new_height = 0;
	int new_width = 0;
	int new_time = 0;
	int new_prob = 20;
	fscanf(new_fptr, "%d", &new_points);
	fscanf(new_fptr, "%d", &new_time);
	fscanf(new_fptr, "%d", &new_width);
	fscanf(new_fptr, "%d", &new_height);
	fscanf(new_fptr, "%d", &new_prob);
	bool change = true;
	if(fptr != 0){
		for(int i = 0; i < 10; i++){
			int points; 
		       	fscanf(fptr, "%d", &points);
			int time; 
			fscanf(fptr, "%d", &time);
			int width; 
			fscanf(fptr, "%d", &width);
			int height;
			fscanf(fptr, "%d", &height);
			int probability; 
			fscanf(fptr, "%d", &probability);
			
			if(new_points > points && change == true){
				fprintf(edit, "%d ", new_points);
				fprintf(edit, "%d ", new_time);
				fprintf(edit, "%d ", new_width);
				fprintf(edit, "%d ", new_height);
				fprintf(edit, "%d\n", new_prob);
				fprintf(edit, "%d ", points);
				fprintf(edit, "%d ", time);
				fprintf(edit, "%d ", width);
				fprintf(edit, "%d ", height);
				fprintf(edit, "%d\n", probability);
				i++;
				change = false;
				
			}else{
				fprintf(edit, "%d ", points);
				fprintf(edit, "%d ", time);
				fprintf(edit, "%d ", width);
				fprintf(edit, "%d ", height);
				fprintf(edit, "%d\n", probability);
			}
		}
	}else{
		fprintf(edit, "%d ", new_points);
		fprintf(edit, "%d ", new_time);
		fprintf(edit, "%d ", new_width);
		fprintf(edit, "%d ", new_height);
		fprintf(edit, "%d ", new_prob);
	}
	fclose(new_fptr);
	new_fptr = fopen("new_highscore.txt", "w+");
	fprintf(new_fptr, "-1 0 0 0 0");
	fclose(edit);
	fclose(fptr);
	fclose(new_fptr);
	
	
	Matrix* high = draw_highscores(matrix);
	matrix_print_update(matrix, high);
	matrix_free(high);
	while(1){
		if(stdin_has_changed()){
			char c = read_from_stdin();
			if(c == 'q'){
				break;
			}
		}
	}
	matrix_resize(matrix, 100, 100, &def);
	Matrix* clean = matrix_new(matrix_width(matrix), matrix_height(matrix), &def);
	matrix_print_update(matrix, clean);
	matrix_free(clean);
	
}
Matrix* starting_menu(){
	
	/* draw the starting menu with New Game, Settings, Highscores and Quit at the top
	 * and Key Binds at the bottom */
	
	Cell def = (Cell){.content = ' ', .background_color = BG_BLACK, .text_color = FG_BLACK}; 
	Matrix* matrix = matrix_new(30, 30, &def);
	matrix_set_str_at(matrix,0, 1, "MINESWEEPER", FG_RED, BG_BLACK);
	matrix_set_str_at(matrix,3, 2, "NEW GAME", FG_YELLOW, BG_BLACK);
	matrix_set_str_at(matrix,3, 3, "SETTINGS", FG_WHITE, BG_BLACK);
	matrix_set_str_at(matrix,3, 4, "HIGHSCORES", FG_WHITE, BG_BLACK);
	matrix_set_str_at(matrix,3, 5, "EXIT", FG_WHITE, BG_BLACK);
	matrix_set_str_at(matrix,0, 8, "SHORTCUTS", FG_RED, BG_BLACK);
	matrix_set_str_at(matrix,3, 9, "Q        EXIT GAME", FG_WHITE, BG_BLACK);
	matrix_set_str_at(matrix,3, 10, "W        NEXT ITEM", FG_WHITE, BG_BLACK);
	matrix_set_str_at(matrix,3, 11, "S        PREVIOUS ITEM", FG_WHITE, BG_BLACK);
	matrix_set_str_at(matrix,3, 12,"SPACE    ACTIVATE ITEM", FG_WHITE, BG_BLACK);
	matrix_set_str_at(matrix, 3, 14, "READ THE README.TXT FIRST!!!!", FG_HI_RED, BG_BLACK);
	return matrix;
}

void useless(){
	int a = 42;
}


void main(){
	
	/* initialise the starting menu with New Game, Settings and Quit */
	
	tui_init();
	Size2 min_term_size = {40, 20};
	Size2 term_size = tui_size();
	if(term_size.x < min_term_size.x || term_size.y < min_term_size.y){
		tui_shutdown();
		printf("ERROR: terminal must be atleast of size 40 x 20.\n");
		exit(1);
	}
	Cell v = (Cell){.content = ' ', .background_color = BG_BLACK, .text_color = FG_BLACK};
	Matrix* matrix = matrix_new(30, 30, &v);
	matrix_print_update(matrix, starting_menu());
	Field* field = field_new();
	bool quit = false;
	FILE* fptr = fopen("new_highscore.txt", "w+");
	fprintf(fptr, "-1 0 0 0 0");
	fclose(fptr);
	while(1){
		if(stdin_has_changed()){
			char c = read_from_stdin(); 
			if(c == 'w' || c == 's'){
				Matrix* new = move_down(matrix,c);
				matrix_print_update(matrix, new);
				matrix_free(new);
			}
			else if(c == 'q'){
				break;
			}
			else if(c == 'e'){
				system("x-www-browser https://www.youtube.com/watch?v=dQw4w9WgXcQ");
				
			}
			else if(c == ' '){
				switch(point){
					case 2: 
						field->lost = false;
						new_game(matrix, field);
						matrix_print_update(matrix, starting_menu());
						matrix_resize(matrix, 300+field->settings.width, 1000+field->settings.height, &v);
						matrix_resize(matrix, 30, 30, &v);
						break;
					case 3: 
					
						setting(matrix,field);
						FILE* fptr = fopen("settings.txt","w");
						fprintf(fptr, "%d\n", field->settings.width);
						fprintf(fptr, "%d\n", field->settings.height);
						fprintf(fptr, "%d\n", field->settings.probability);
						fclose(fptr);
						matrix_print_update(matrix, starting_menu());
						point = 2;
						break;
					case 4: 
						/*useless();
						FILE* f = fopen("new_highscore.txt","r+");
						if(fptr == 0){
							f = fopen("new_highscore.txt","w+");
							fprintf(f, "-1 0 0 0 0");
						}
						fclose(f);*/
						highscores(matrix, field); 
						matrix_resize(matrix, 30, 30, &v);
						matrix_print_update(matrix, starting_menu());
						point = 2;
						break;
					case 5: 
						quit = true;
						break;
					default: continue;
				}
			}if(quit){
				break;
			}

		}
		
	}
	matrix_free(matrix);
	field_free(field);
	tui_shutdown();

}
