#pragma once
#include <vector>
#define MYGOMOKU_API __declspec(dllexport)

class GomokuApi
{
public:
	GomokuApi();
	~GomokuApi();

	int GetTurn() const;
	bool CanIPutHere();
	int GetDeletedPion();
	int GetNbWhitePrise() const;
	int GetNbBlackPrise() const;
	int GetVictoryTeam() const;
	int GetVictory() const;


private:
	int _turn;
	int _nbPriseW;
	int _nbPriseB;
	int _victoryTeam;
	bool _isVictory;
	std::vector<int> _deletedPion;
};

extern "C" {
	MYGOMOKU_API int Add(int a, int b);
	MYGOMOKU_API GomokuApi* CreateGomokuAPI();
	MYGOMOKU_API void DeleteGomokuAPI(GomokuApi *api);
	MYGOMOKU_API int GetTurn(GomokuApi *api);
	MYGOMOKU_API bool CanIPutHere(GomokuApi *api, int pos);
	MYGOMOKU_API int GetDeletedPion(GomokuApi *api);
	MYGOMOKU_API int GetNbWhitePrise(GomokuApi *api);
	MYGOMOKU_API int GetNbBlackPrise(GomokuApi *api);
	MYGOMOKU_API int GetVictoryTeam(GomokuApi *api);
	MYGOMOKU_API bool GetVictory(GomokuApi *api);
}

