#pragma once

#include <iostream>
#include <map>
#include <list>
#include <vector>
#include <iostream>     // std::cout
#include <algorithm>    // std::find
#include <vector>
#include <fstream>
#include "MapGomoku.h"

// define MYGOMOKU_API __declspec(dllexport)
# define MYGOMOKU_API __attribute__((visibility("default")))
# define MY_ABS(nb) ((nb<0)?(nb):(-nb))
# define PATTERN(pattern) ((ret = ((tmp = line.find(pattern)) != std::string::npos)?(tmp):(ret)) != std::string::npos)

typedef std::pair<int, int> pair;

namespace board
{
	enum class    Direction : uint16_t
	{
		N = 0,
		NE = 1,
		E = 2,
		SE = 3
	};
}

class GomokuApi
{
public:
	GomokuApi();
	~GomokuApi();
	GomokuApi	&operator=(const GomokuApi &copy) { return *this; }
	std::ofstream fichier;

	Map_gomoku get_board() const;
	void  setMap(uint64_t *);
	int     **check_if_can_take(int x, int y);
	int     get_turn() const;
	void    change_turn();
	int     put_piece(int, int, int);
	void    test();
	int     check_5_align_board()const;
	bool    is_double_3_align(int x, int y, int color) const;

	bool CanIPutHere(int pos);
	int GetDeletedPion();
	bool GetVictoryTeam() const;
	bool GetVictory() const;

//	std::map<std::pair<int, int>, bool> get_board() const;
	void special_move(int x, int y, int color);
	//bool get_turn() const;
	void set_turn();
	//int     check_5_align_board() const;
	//int  **check_if_can_take(std::pair<int, int>);
	//int  check_pieces_taken(std::pair<int, int>, std::pair<int, int>, std::pair<int, int>, int **);


	void	set3Rule(bool val);
	void	set3BreakRule(bool val);
	int									getError() { return _error; }

private:
	pair    is_3_align(int x, int y, int inc_x, int inc_y, int color) const;
	bool    check_5_align(pair, std::list<pair> &) const;
	//int     **check_if_can_take(std::pair<int, int>);
	int     check_pieces_taken(std::pair<int, int>, std::pair<int, int>, std::pair<int, int>, int **);
	std::list<pair>    check_line_align(int, int, int, int) const;
	bool    check_line_breakable(const std::list<pair> &) const;
	bool    check_if_out(int, int) const;
	std::list<pair>    check_if_vulnerable(int, int) const;

	Map_gomoku				_board;
	int                                 _color;
	int                                 _pieces_taken[2];






	//bool put_piece(std::pair<int, int>, bool);
	//bool check_if_free(std::pair<int, int>);
//	pair    is_3_align(int x, int y, pair inc, bool color) const;
	//bool    check_5_align(pair, std::list<pair> &) const;
	//bool    check_if_free(std::pair<int, int>) const;
	//bool    is_double_3_align(int x, int y, bool color) const;
	//bool check_if_free_cst(std::pair<int, int>) const;
	//std::list<pair>    check_line_align(int, int, int, int) const;
	//std::list<pair>    check_if_vulnerable(pair) const;
	//bool check_line_breakable(std::list<pair>) const;
	//bool    check_if_out(pair) const;

	bool								_victoryTeam;
	bool								_isVictory;
	//std::map<std::pair<int, int>, bool>	_board;
	//bool								_color;
	std::vector<int>					_deletedPion;
	bool								_is3rule;
	bool								_isBreakRule;
	int									_error;
};

extern "C" {
	MYGOMOKU_API int Add(int a, int b);
	MYGOMOKU_API GomokuApi* CreateGomokuAPI();
	MYGOMOKU_API void DeleteGomokuAPI(GomokuApi *api);
	MYGOMOKU_API bool GetTurn(GomokuApi *api);
	MYGOMOKU_API void SetTurn(GomokuApi *api);
	MYGOMOKU_API bool CanIPutHere(GomokuApi *api, int pos);
	MYGOMOKU_API int GetDeletedPion(GomokuApi *api);
	MYGOMOKU_API bool GetVictoryTeam(GomokuApi *api);
	MYGOMOKU_API bool GetVictory(GomokuApi *api);
	MYGOMOKU_API void Opt3Rule(GomokuApi *api, bool val);
	MYGOMOKU_API int OptBreakRule(GomokuApi *api, bool val);
	MYGOMOKU_API void ChangeMap(GomokuApi *api, int x, int y, int color);
	MYGOMOKU_API int GetError(GomokuApi *api);
}

