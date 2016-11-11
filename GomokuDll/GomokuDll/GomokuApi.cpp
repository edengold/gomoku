#include "GomokuApi.h"

MYGOMOKU_API int Add(int a, int b)
{
	return a + b;
}

MYGOMOKU_API GomokuApi* CreateGomokuAPI()
{
	return new GomokuApi();
}

MYGOMOKU_API void DeleteGomokuAPI(GomokuApi *api)
{
	delete api;
}

MYGOMOKU_API int GetTurn(GomokuApi *api)
{
	return api->GetTurn();
}

MYGOMOKU_API bool CanIPutHere(GomokuApi *api, int pos)
{
	return api->CanIPutHere();
}

MYGOMOKU_API int GetDeletedPion(GomokuApi *api)
{
	return api->GetDeletedPion();
}

MYGOMOKU_API int GetNbWhitePrise(GomokuApi *api)
{
	return api->GetNbWhitePrise();	
}

MYGOMOKU_API int GetNbBlackPrise(GomokuApi *api)
{
	return api->GetNbBlackPrise();
}

MYGOMOKU_API int GetVictoryTeam(GomokuApi *api)
{
	return api->GetVictoryTeam();
}

MYGOMOKU_API bool GetVictory(GomokuApi *api)
{
	return  api->GetVictory();
}


GomokuApi::GomokuApi()
{
	_turn = 0;
	_nbPriseB = 0;
	_nbPriseW = 0;
	_isVictory = false;
	_victoryTeam = -1;
}

GomokuApi::~GomokuApi()
{
}

int GomokuApi::GetTurn() const
{
	return (_turn);
}

bool GomokuApi::CanIPutHere()
{
	//bool -> if i can put pion here 
	if (true)
	{
		//func to checkprise si oui ajouter au tab des pion a delete
		//check si victoire
		_turn = !_turn;
		return true;
	}
	return false;
}

int GomokuApi::GetDeletedPion()
{
	if (!_deletedPion.empty())
	{
		int tmp = _deletedPion.at(_deletedPion.size());
		_deletedPion.pop_back();
		return tmp;
	}
	return -1;
}

int GomokuApi::GetNbWhitePrise() const
{
	return _nbPriseW;
}

int GomokuApi::GetNbBlackPrise() const
{
	return _nbPriseB;
}

int GomokuApi::GetVictoryTeam() const
{
	return _victoryTeam;
}

int GomokuApi::GetVictory() const
{
	return _isVictory;
}
