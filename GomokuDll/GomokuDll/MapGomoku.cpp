#include "MapGomoku.h"

Map_gomoku::Map_gomoku()
{
	_map = new uint64_t[MAP_H];

	for (int x = 0; x < MAP_H; x++)
	{
		//_map[x] = 0;
		for (int y = 0; y < MAP_H; y++)
			setPiece(x, y, EMPTY);
	}
	fichierMap.open("testMap.txt", std::ios::out | std::ios::trunc);
	fichierMap << "LOG START\n";
	printMap();
}

Map_gomoku::Map_gomoku(const Map_gomoku &copy)
{
	_map = new uint64_t[MAP_H];
	memcpy(_map, copy._map, MAP_H * sizeof(uint64_t));
}

Map_gomoku::~Map_gomoku()
{
	//delete _map;
}

int     Map_gomoku::getPiece(int x, int y) const
{
	return ((_map[y] >> (x * 2)) & 3);
}

int     Map_gomoku::setPiece(int x, int y, int type)
{
	/*uint64_t mask = 0;
	uint64_t newline = 0;
	uint64_t v = 3;
	uint64_t t = type;
	mask |= (v << (x * 2));
	newline = (t << (x * 2));
	_map[y] = (_map[y] & ~mask) | (newline & mask);*/
	PUT_PIECE(_map, x, y, type);
	return 0;
}

void    Map_gomoku::printMap() 
{
	fichierMap << "\n";
	for (int y = 0; y < MAP_H; y++)
	{
		for (int x = 0; x < MAP_H; x++)
		{
			if (getPiece(x, y) == WHITE)
				fichierMap << "W";
			else if (getPiece(x, y) == BLACK)
				fichierMap << "B";
			else
				fichierMap << " ";
			if (x < (MAP_H - 1))
				fichierMap << "*";
		}
		fichierMap << std::endl;
	}

}

bool    Map_gomoku::isEmpty(int x, int y) const
{
	if (x >= MAP_H || x < 0 || y >= MAP_H || y < 0)
		return false;
	if (getPiece(x, y) == EMPTY)
		return true;
	return false;
}

uint64_t    *Map_gomoku::getMap() const
{
	return (_map);
}

void	Map_gomoku::setMap(uint64_t *map)
{
	_map = map;
}