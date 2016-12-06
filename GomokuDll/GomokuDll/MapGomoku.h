#pragma once

#include <iostream>
#include <cstdint>
#include <bitset>
#include <fstream>

#define MAP_H 19
#define BLACK 3
#define WHITE 1
#define EMPTY 0

class   Map_gomoku
{
public:
	Map_gomoku();
	~Map_gomoku();
	std::ofstream fichierMap;
	int         getPiece(int, int) const;
	void         setPiece(int, int, int);
	void        printMap() ;
	bool        isEmpty(int, int) const;
	uint64_t    *getMap() const;
private:
	uint64_t    *_map;
};

