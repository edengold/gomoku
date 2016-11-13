#include "GomokuApi.h"

#pragma region ForDll
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

MYGOMOKU_API bool GetTurn(GomokuApi *api)
{
	return api->get_turn();
}

MYGOMOKU_API bool CanIPutHere(GomokuApi *api, int pos)
{
	return api->CanIPutHere(pos);
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

MYGOMOKU_API bool GetVictoryTeam(GomokuApi *api)
{
	return api->GetVictoryTeam();
}

MYGOMOKU_API bool GetVictory(GomokuApi *api)
{
	return  api->GetVictory();
}

#pragma endregion

GomokuApi::GomokuApi()
{
	_color = true;
	_nbPriseB = 0;
	_nbPriseW = 0;
	_isVictory = false;
	_victoryTeam = false;
}

GomokuApi::~GomokuApi()
{
}

#pragma region ForAPI

std::map<std::pair<int, int>, bool> GomokuApi::get_board() const
{
    return _board;
}

bool GomokuApi::get_turn() const
{
    return _color;
}
/*
bool GomokuApi::move(int x, int y, bool color)
{
    std::pair<int, int> pos(x, y);

    if (color != _color)
        return false;
        // all function call to check
    return put_piece(pos, color);
}
*/
bool GomokuApi::check_if_free(std::pair<int, int> pos)
{
	if (_board.find(pos) == _board.end())
		return true;
	return false;
}

bool GomokuApi::check_if_free_cst(std::pair<int, int> pos, std::map<pair, bool> board) const
{
    if (!board[pos])
        return true;
    return false;
}

bool GomokuApi::put_piece(std::pair<int, int> pos, bool color)
{
    _board[pos] = color;
    return true;
}

bool GomokuApi::check_5_align(int x, int y) const
{
  bool check = false;

  if (!(get_board()[std::pair<int, int>(x, y)]))
    return false;
  for (int rotation = 0 ; rotation < 4 ; rotation++)
  	check = check || check_line_align((rotation + 2) / 3,
	  (rotation == 0) ? (1) : (2 - rotation), x, y);
  return check;
}

bool GomokuApi::check_line_align(int x_inc, int y_inc, int x, int y) const
{
  std::map<std::pair<int, int>, bool>	board = get_board();
  bool					color = board[std::pair<int, int>(x, y)];
  std::list<std::pair<int, int>>	line;
  bool					cursor = color;

  while (cursor == color)
    {
      x += x_inc;
      y += y_inc;
      cursor = (check_if_free_cst(pair(x, y), board) == true)?(!color):(board[pair(x, y)]);
    }
  x -= x_inc; y -= y_inc; cursor = color;
  while (cursor == color)
    {
      line.push_back(std::pair<int, int>(x, y));
      x -= x_inc;
      y -= y_inc;
      cursor = (check_if_free_cst(pair(x, y), board) == true)?(!color):(board[pair(x, y)]);
    }
  if (line.size() < 5)
    return (false);
  return (check_line_breakable(line, board, x_inc, y_inc));
}

bool GomokuApi::check_line_breakable(std::list<pair> list, std::map<pair,bool> board, int x_inc, int y_inc) const
{
  bool		color = board[list.front()];
  unsigned int	cpt = 1;

  for (std::list<pair>::iterator it = list.begin() ; it != list.end() ; it++)
    {
      if (check_column_breakable(*it, board, y_inc, -x_inc, color) == true)
	if (((list.size() - cpt) < cpt)?(cpt):(list.size() - cpt) < 5)
	  return (true);
      cpt++;
    }
  return (false);
}

bool GomokuApi::check_column_breakable(pair coor, const std::map<pair,bool> board,
				     int x_inc, int y_inc, bool color) const
{
  bool		cursor = color;
  int		x = coor.first;
  int		y = coor.second;
  std::string	pattern = "";
  bool		end = false;

  while (check_if_free_cst(pair(x, y), board) == false &&
	 MY_ABS(coor.first - x) < 3 && MY_ABS(coor.second - y) < 3)
    {
      x -= x_inc;
      y -= y_inc;
    }
  if (!(MY_ABS(coor.first - x) < 3 && MY_ABS(coor.second - y) < 3))
    end = true;
  x += x_inc;
  y += y_inc;
  while (check_if_free_cst(pair(x, y), board) == false &&
	 MY_ABS(coor.first - x) < 3 && MY_ABS(coor.second - y) < 3)
    {
      pattern.append((cursor==color)?("s"):("o"));
      x -= x_inc;
      y -= y_inc;
    }
  if (!(MY_ABS(coor.first - x) < 3 && MY_ABS(coor.second - y) < 3))
    end = true;
  if ((pattern.compare("sso") == 0 || pattern.compare("oss") == 0)
      && end == true)
    return (true);
  return (false);
}

bool GomokuApi::is_3_align(int x, int y, board::Direction dir, bool color) const
{
  pair			inc = pair((static_cast<int>(dir) + 2) / 3,
				   (static_cast<int>(dir) == 0)?(1):
				   (2 - static_cast<int>(dir)));
  std::string		line;
  std::map<pair, bool>	board = get_board();

  x = x + 5 * inc.first;
  y = y + 5 * inc.second;
  for (int i = 0 ; i < 9 ; i++)
    {
      x -= inc.first;
      y -= inc.second;
      if (check_if_free_cst(pair(x, y), board) == true)
	line.append("f");
      else if (board[pair(x, y)] == color)
	line.append("s");
      else
	line.append("o");
    }
  return(analyse_3_align(line));
}

bool GomokuApi::analyse_3_align(const std::string line) const
{
  if (line.compare("fffffssff") == 0 || line.compare("ffssfffff") == 0
      || line.compare("fffffsfsf") == 0 || line.compare("fsfsfffff") == 0
      || line.compare("ffffffssf") == 0 || line.compare("fssffffff") == 0
      || line.compare("fffsffsff") == 0 || line.compare("ffsffsfff")
      || line.compare("fffsfsfff"))
    return (true);
  return (false);
}

#pragma endregion 

#pragma region ForCommunication

bool GomokuApi::CanIPutHere(int pos)
{
	//bool -> if i can put pion here 
	if (true)
	{
		//func to checkprise si oui ajouter au tab des pion a delete
		//check si victoire
		int x = static_cast<int>(pos % 20);
		int y = static_cast<int>(pos / 20);
		std::pair<int, int> tmp(x, y);
		put_piece(tmp, _color);
		int                 **tab;

		tab = move(x, y, _color);
		if (tab != NULL && tab[0][0] != -1)
			{
				int i = 0;
				while (tab[i])
				{
					if (tab[i][0] == -1)
						break;
					if (tab[i][0] && tab[i][0] != -1)
					{
						_deletedPion.push_back((tab[i][1] * 20) + tab[i][0]);
					}
					i++;
				}
			}
		if (check_5_align(x, y))
		{
			_isVictory = true;
			_victoryTeam = _color;
			return true;
		}
		_color = !_color;
		return true;
	}
	return false;
}

int GomokuApi::GetDeletedPion()
{
	if (!_deletedPion.empty())
	{
		int tmp = _deletedPion.at(_deletedPion.size() - 1);
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

bool GomokuApi::GetVictoryTeam() const
{
	return _victoryTeam;
}

bool GomokuApi::GetVictory() const
{
	return _isVictory;
}
#pragma endregion 

int **GomokuApi::move(int x, int y, bool color)
{
	std::pair<int, int> pos(x, y);
	int                 **tab;

	//put_piece(pos, color);
	// temporaire
	tab = check_if_can_take(pos);
	return tab;
}
int    GomokuApi::check_pieces_taken(std::pair<int, int> one, std::pair<int, int> two, std::pair<int, int> three, int **tab)
{
	if (check_if_free(one) || check_if_free(two) || check_if_free(three))
		return 0;
	if (_board[one] == _color && _board[two] != _color && _board[three] != _color)
	{
		tab[0][0] = two.first;
		tab[0][1] = two.second;
		_board.erase(_board.find(two));
		tab[1][0] = three.first;
		tab[1][1] = three.second;
		_board.erase(_board.find(three));
	}
	return 2;
}


int **GomokuApi::check_if_can_take(std::pair<int, int> pos)
{
	int                 count;
	int                 **tab;

	count = 0;
	tab = new int*[20];
	for (int i = 0; i < 20; ++i)
		tab[i] = new int[2];
	count += check_pieces_taken(pair(pos.first + 3, pos.second), pair(pos.first + 2, pos.second), pair(pos.first + 1, pos.second), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second), pair(pos.first - 2, pos.second), pair(pos.first - 1, pos.second), &tab[count]);
	count += check_pieces_taken(pair(pos.first, pos.second + 3), pair(pos.first, pos.second + 2), pair(pos.first, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first, pos.second - 3), pair(pos.first, pos.second - 2), pair(pos.first, pos.second - 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first + 3, pos.second + 3), pair(pos.first + 2, pos.second + 2), pair(pos.first + 1, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second - 3), pair(pos.first - 2, pos.second - 2), pair(pos.first - 1, pos.second - 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first - 3, pos.second + 3), pair(pos.first - 2, pos.second + 2), pair(pos.first - 1, pos.second + 1), &tab[count]);
	count += check_pieces_taken(pair(pos.first + 3, pos.second - 3), pair(pos.first + 2, pos.second - 2), pair(pos.first + 1, pos.second - 1), &tab[count]);
	tab[count][0] = -1;
	return tab;
}
void    GomokuApi::test()
{
	int **tab;
	put_piece(pair(0, 0), true);
	put_piece(pair(0, 2), true);
	put_piece(pair(1, 0), false);
	put_piece(pair(2, 0), false);

	_color = true;
	tab = move(3, 0, true);
	std::cout << tab[0][0] << std::endl;
	std::cout << tab[0][1] << std::endl;
	std::cout << tab[1][0] << std::endl;
	std::cout << tab[1][1] << std::endl;
}
/*
int main()
{
	GomokuApi gomoku;
	//gomoku.test();
	gomoku.CanIPutHere(0);
	gomoku.CanIPutHere(1);
	gomoku.CanIPutHere(12);
	gomoku.CanIPutHere(2);
	gomoku.CanIPutHere(3);
	gomoku.CanIPutHere(407);
	gomoku.CanIPutHere(3);
	gomoku.CanIPutHere(400);
	gomoku.CanIPutHere(4);
	std::map<std::pair<int, int>, bool>	board = gomoku.get_board();
	for (std::map<std::pair<int, int>, bool>::iterator it = board.begin(); it != board.end(); ++it)
	{
		printf("x = %d y = %d =>", it->first.first, it->first.second);
		if (it->second)
			printf("blanc\n");
		else
		{
			printf("noir\n");
		}
		it->second; // accede à la valeur
	}
	if (gomoku.check_5_align(0,0))
	{
		printf("c aligner");
	}
	for (int i = 0; i < gomoku._deletedPion.size(); i++)
	{
		printf("%d ", gomoku._deletedPion[i]);
	}
	printf("\n%d ", gomoku.GetDeletedPion());
	printf("\n%d ", gomoku.GetDeletedPion());
	printf("\n%d ", gomoku.GetDeletedPion());
	while (true)
	{
		
	}
	return 0;
}*/