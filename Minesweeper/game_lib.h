#ifndef GAME_LIB_H
#define GAME_LIB_H

#include "vec.h"
#include "game_structure.h"
#include <stddef.h>

void mine_lay(Field* field);

Vec* create_cells(Field* field);

void set_flag(Field* field);

void** reveal_cell(Field* field);

Field* field_new();

Field* read_settings(Field* field);

Zelle* zelle_at(Field* field, int x, int y);

void flag_set(Field* field, Matrix* matrix);

char int_convert(int x);

void count_number(Field* field, Zelle* zelle, int x, int y);

Matrix* draw_cell(Matrix* matrix, Field* field, Zelle* zelle);

void clean_cells(Field* field);

Matrix* draw_field(Field* field);

Matrix* movement(Field* field, Matrix* matrix, char c);

void new_game(Matrix* matrix, Field* field);

void field_free(Field* field);

#endif
