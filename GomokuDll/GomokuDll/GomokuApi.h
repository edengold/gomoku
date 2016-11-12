#pragma once

#include <iostream>
#include <map>
#include <list>
#include <vector>

#define MYGOMOKU_API __declspec(dllexport)
#define MY_ABS(nb) ((nb<0)?(nb):(-nb))

typedef std::pair<int, int> pair;

namespace board
{
	enum class	Direction : uint16_t
	{
		N = 0,
		NE = 1,
		E = 2,
		S = 3
	};
}

class GomokuApi
{
public:
	GomokuApi();
	~GomokuApi();

	bool CanIPutHere(int pos);
	int GetDeletedPion();
	int GetNbWhitePrise() const;
	int GetNbBlackPrise() const;
	bool GetVictoryTeam() const;
	bool GetVictory() const;

	std::map<std::pair<int, int>, bool> get_board() const;
	bool move(int x, int y, bool color);
	bool get_turn() const;
	bool check_5_align(int x, int y) const;
	bool is_3_align(int x, int y, board::Direction dir, bool color) const;

private:
	int _nbPriseW;
	int _nbPriseB;
	bool _victoryTeam;
	bool _isVictory;
	std::vector<int> _deletedPion;

	bool put_piece(std::pair<int, int>, bool);
	bool check_if_free(std::pair<int, int>);
	bool check_if_free_cst(std::pair<int, int>, std::map<pair, bool>) const;
	bool check_line_align(int, int, int, int) const;
	bool check_line_breakable(std::list<pair>, std::map<pair, bool>, int, int) const;
	bool check_column_breakable(pair, const std::map<pair, bool>, int, int, bool) const;
	bool analyse_3_align(const std::string) const;

	std::map<std::pair<int, int>, bool>	_board;
	bool					_color;
};

extern "C" {
	MYGOMOKU_API int Add(int a, int b);
	MYGOMOKU_API GomokuApi* CreateGomokuAPI();
	MYGOMOKU_API void DeleteGomokuAPI(GomokuApi *api);
	MYGOMOKU_API bool GetTurn(GomokuApi *api);
	MYGOMOKU_API bool CanIPutHere(GomokuApi *api, int pos);
	MYGOMOKU_API int GetDeletedPion(GomokuApi *api);
	MYGOMOKU_API int GetNbWhitePrise(GomokuApi *api);
	MYGOMOKU_API int GetNbBlackPrise(GomokuApi *api);
	MYGOMOKU_API bool GetVictoryTeam(GomokuApi *api);
	MYGOMOKU_API bool GetVictory(GomokuApi *api);
}

