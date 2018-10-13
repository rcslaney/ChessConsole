using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChessConsole {
	[Serializable]
	public class Piece {
		public int id;
		public int value;
		public char character;

		public Piece(int inputId) {
			id = inputId;
			switch (Math.Abs(id)) {
				case 1: // Pawn
					value = 1;
					character = (char)9823;
					break;
				case 2: // Knight
					value = 3;
					character = (char)9822;
					break;
				case 3: // Bishop
					value = 3;
					character = (char)9821;
					break;
				case 4: // Rook
					value = 5;
					character = (char)9820;
					break;
				case 5: // Queen
					value = 9;
					character = (char)9819;
					break;
				case 6: // King
					value = 1000;
					character = (char)9818;
					break;
			}
		}
	}

	public class MyEqualityComparer : IEqualityComparer<int[]> {
		public bool Equals(int[] x, int[] y) {
			if (x.Length != y.Length) {
				return false;
			}
			for (int i = 0; i < x.Length; i++) {
				if (x[i] != y[i]) {
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(int[] obj) {
			int result = 17;
			for (int i = 0; i < obj.Length; i++) {
				unchecked {
					result = result * 23 + obj[i];
				}
			}
			return result;
		}
	}
	
	[Serializable]
	public class Chessboard {
		public static T DeepCopy<T>(T other) {
			using (MemoryStream ms = new MemoryStream()) {
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(ms, other);
				ms.Position = 0;
				return (T)formatter.Deserialize(ms);
			}
		}

		public Piece[,] board;
		public Piece[] takenPieces;

		public Chessboard() {
			int[,] tempBoard = new int[,] {
				{  4,  2,  3,  5,  6,  3,  2,  4 },
				{  1,  1,  1,  1,  1,  1,  1,  1 },
				{  0,  0,  0,  0,  0,  0,  0,  0 },
				{  0,  0,  0,  0,  0,  0,  0,  0 },
				{  0,  0,  0,  0,  0,  0,  0,  0 },
				{  0,  0,  0,  0,  0,  0,  0,  0 },
				{ -1, -1, -1, -1, -1, -1, -1, -1 },
				{ -4, -2, -3, -5, -6, -3, -2, -4 }
			};

			board = new Piece[8, 8];

			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < 8; j++) {
					if (tempBoard[i, j] == 0) {
						board[i, j] = null;
					} else {
						board[i, j] = new Piece(tempBoard[i, j]);
					}
				}
			}
		}

		public int evaluateBoard() {
			int score = 0;
			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < 8; j++) {
					if (board[i, j] != null) {
						score += board[i, j].value * Math.Sign(board[i, j].id);
					}
				}
			}
			return score;
		}

		public void displayBoard(int player) {
			if (player == 0) {
				for (int no = 0; no < 8; no++) {
					for (int letter = 0; letter < 8; letter++) {
						Console.Write(((char)(65 + letter)).ToString() + no + " ");
					}
					Console.WriteLine(" ");
				}
			} else if (player == 1) {
				for (int no = 7; no >= 0; no--) {
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.White;
					Console.Write(" " + (no + 1) + " ");
					for (int letter = 0; letter < 8; letter++) {
						Console.BackgroundColor = ((no + letter) % 2 == 1) ? ConsoleColor.Gray : ConsoleColor.DarkGray;
						if (board[no, letter] == null) {
							Console.Write("  ");
						} else {
							Console.ForegroundColor = (board[no, letter].id < 0) ? ConsoleColor.Black : ConsoleColor.White;
							if (Math.Abs(board[no, letter].id) == 5) {
								Console.Write(board[no, letter].character);
							} else {
								Console.Write(board[no, letter].character + "");
							}
						}
					}
					Console.BackgroundColor = ConsoleColor.Black;
					Console.WriteLine(" ");
				}
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("   a b c d e f g h ");
			} else if (player == -1) {
				for (int no = 0; no < 8; no++) {
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.White;
					Console.Write(" " + (no + 1) + " ");
					for (int letter = 7; letter >= 0; letter--) {
						Console.BackgroundColor = ((no + letter) % 2 == 1) ? ConsoleColor.Gray : ConsoleColor.DarkGray;
						if (board[no, letter] == null) {
							Console.Write("  ");
						} else {
							Console.ForegroundColor = (board[no, letter].id < 0) ? ConsoleColor.Black : ConsoleColor.White;
							Console.Write(board[no, letter].character + " ");
						}
					}
					Console.BackgroundColor = ConsoleColor.Black;
					Console.WriteLine(" ");
				}
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("   h g f e d c b a ");
			}
		}

		public int stringMakeMove(String moveStr, int player) {
			Regex validMove = new Regex("^[a-hA-H][1-8]-[a-hA-H][1-8]$");
			if (!validMove.IsMatch(moveStr)) {
				// Console.WriteLine("Terrible news everybody! Could not move the character");
				return 0;
			} else {
				int[][] move = new int[2][];
				move[0] = new int[2];
				move[1] = new int[2];
				for (int part = 0; part < 5; part++) {
					switch (part) {
						case 0:
							move[0][1] = ((int)moveStr[part] % 16) - 1;
							break;
						case 1:
							move[0][0] = ((int)moveStr[part] % 48) - 1;
							break;
						case 3:
							move[1][1] = ((int)moveStr[part] % 16) - 1;
							break;
						case 4:
							move[1][0] = ((int)moveStr[part] % 48) - 1;
							break;
					}
				}

				// Dictionary<int[], int[]> legalMoves = getLegalMoves(player, true);
				// if(legalMoves.ContainsKey(move[0]) && legalMoves) {
				// 
				// }
				board[move[1][0], move[1][1]] = board[move[0][0], move[0][1]];
				board[move[0][0], move[0][1]] = null;

				return 1;
			}
		}

		public Dictionary<int[], List<int[]>> getLegalMoves(int player, bool checkKing) {
			Dictionary<int[], List<int[]>> legalMoves = new Dictionary<int[], List<int[]>>( new MyEqualityComparer() );

			bool possible;
			int count;

			int[] currentKingPosition = { 0, 0 };

			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < 8; j++) {
					if (board[i, j] != null && Math.Sign(board[i, j].id) == player) {
						legalMoves[new int[] { i, j }] = new List<int[]>();
						Piece currentPiece = board[i, j];
						switch (currentPiece.id) {
							case 1: // White Pawn
								if (i < 7 && board[i + 1, j] == null) {
									legalMoves[new int[] { i, j }].Add(new int[] { i + 1, j });
								}
								if (i == 1 && board[i + 1, j] == null && board[i + 2, j] == null) {
									legalMoves[new int[] { i, j }].Add(new int[] { i + 2, j });
								}
								if (j > 0 && i < 7 && board[i + 1, j - 1] != null && Math.Sign(board[i + 1, j - 1].id) != player) {
									legalMoves[new int[] { i, j }].Add(new int[] { i + 1, j - 1 });
								}
								if (j < 7 && i < 7 && board[i + 1, j + 1] != null && Math.Sign(board[i + 1, j + 1].id) != player) {
									legalMoves[new int[] { i, j }].Add(new int[] { i + 1, j + 1 });
								}
								break;
							case -1:
								if (i > 0 && board[i - 1, j] == null) {
									legalMoves[new int[] { i, j }].Add(new int[] { i - 1, j });
								}
								if (i == 6 && board[i - 1, j] == null && board[i - 2, j] == null) {
									legalMoves[new int[] { i, j }].Add(new int[] { i - 2, j });
								}
								if (j > 0 && i > 0 && board[i - 1, j - 1] != null && Math.Sign(board[i - 1, j - 1].id) != player) {
									legalMoves[new int[] { i, j }].Add(new int[] { i - 1, j - 1 });
								}
								if (j < 7 && i > 0 && board[i - 1, j + 1] != null && Math.Sign(board[i - 1, j + 1].id) != player) {
									legalMoves[new int[] { i, j }].Add(new int[] { i - 1, j + 1 });
								}
								break;
							case 2: // Knight
							case -2:
								if (i < 6) {
									if (j < 7 && (board[i + 2, j + 1] == null || Math.Sign(board[i + 2, j + 1].id) != player)) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + 2, j + 1 });
									}
									if (j > 0 && (board[i + 2, j - 1] == null || Math.Sign(board[i + 2, j - 1].id) != player)) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + 2, j - 1 });
									}
								}
								if (i > 1) {
									if (j < 7 && (board[i - 2, j + 1] == null || Math.Sign(board[i - 2, j + 1].id) != player)) {
										legalMoves[new int[] { i, j }].Add(new int[] { i - 2, j + 1 });
									}
									if (j > 0 && (board[i - 2, j - 1] == null || Math.Sign(board[i - 2, j - 1].id) != player)) {
										legalMoves[new int[] { i, j }].Add(new int[] { i - 2, j - 1 });
									}
								}
								if (j < 6) {
									if (i < 7 && (board[i + 1, j + 2] == null || Math.Sign(board[i + 1, j + 2].id) != player)) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + 1, j + 2 });
									}
									if (i > 0 && (board[i - 1, j + 2] == null || Math.Sign(board[i - 1, j + 2].id) != player)) {
										legalMoves[new int[] { i, j }].Add(new int[] { i - 1, j + 2 });
									}
								}
								if (j > 1) {
									if (i < 7 && (board[i + 1, j - 2] == null || Math.Sign(board[i + 1, j - 2].id) != player)) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + 1, j - 2 });
									}
									if (i > 0 && (board[i - 1, j - 2] == null || Math.Sign(board[i - 1, j - 2].id) != player)) {
										legalMoves[new int[] { i, j }].Add(new int[] { i - 1, j - 2 });
									}
								}
								break;
							case 3: // Bishop
							case -3:
								possible = true;
								count = 1;
								while (possible && i + count < 8 && j + count < 8) {
									if (board[i + count, j + count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j + count });
									} else {
										if (Math.Sign(board[i + count, j + count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j + count });
										}
										possible = false;
									}
									count++;
								}

								possible = true;
								count = -1;
								while (possible && i + count >= 0 && j + count >= 0) {
									if (board[i + count, j + count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j + count });
									} else {
										if (Math.Sign(board[i + count, j + count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j + count });
										}
										possible = false;
									}
									count--;
								}


								possible = true;
								count = 1;
								while (possible && i + count < 8 && j - count >= 0) {
									if (board[i + count, j - count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j - count });
									} else {
										if (Math.Sign(board[i + count, j - count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j - count });
										}
										possible = false;
									}
									count++;
								}


								possible = true;
								count = -1;
								while (possible && i + count >= 0 && j - count < 8) {
									if (board[i + count, j - count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j - count });
									} else {
										if (Math.Sign(board[i + count, j - count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j - count });
										}
										possible = false;
									}
									count--;
								}
								break;
							case 4: // Rook
							case -4:
								possible = true;
								count = 1;
								while (possible && i + count < 8) {
									if (board[i + count, j] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j });
									} else {
										if (Math.Sign(board[i + count, j].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j });
										}
										possible = false;
									}
									count++;
								}

								possible = true;
								count = -1;
								while (possible && i + count >= 0) {
									if (board[i + count, j] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j });
									} else {
										if (Math.Sign(board[i + count, j].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j });
										}
										possible = false;
									}
									count--;
								}

								possible = true;
								count = 1;
								while (possible && j + count < 8) {
									if (board[i, j + count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i, j + count });
									} else {
										if (Math.Sign(board[i, j + count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i, j + count });
										}
										possible = false;
									}
									count++;
								}

								possible = true;
								count = -1;
								while (possible && j + count >= 0) {
									if (board[i, j + count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i, j + count });
									} else {
										if (Math.Sign(board[i, j + count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i, j + count });
										}
										possible = false;
									}
									count--;
								}
								break;
							case 5: // Queen
							case -5:
								possible = true;
								count = 1;
								while (possible && i + count < 8 && j + count < 8) {
									if (board[i + count, j + count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j + count });
									} else {
										if (Math.Sign(board[i + count, j + count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j + count });
										}
										possible = false;
									}
									count++;
								}

								possible = true;
								count = -1;
								while (possible && i + count >= 0 && j + count >= 0) {
									if (board[i + count, j + count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j + count });
									} else {
										if (Math.Sign(board[i + count, j + count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j + count });
										}
										possible = false;
									}
									count--;
								}


								possible = true;
								count = 1;
								while (possible && i + count < 8 && j - count >= 0) {
									if (board[i + count, j - count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j - count });
									} else {
										if (Math.Sign(board[i + count, j - count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j - count });
										}
										possible = false;
									}
									count++;
								}


								possible = true;
								count = -1;
								while (possible && i + count >= 0 && j - count < 8) {
									if (board[i + count, j - count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j - count });
									} else {
										if (Math.Sign(board[i + count, j - count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j - count });
										}
										possible = false;
									}
									count--;
								}

								possible = true;
								count = 1;
								while (possible && i + count < 8) {
									if (board[i + count, j] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j });
									} else {
										if (Math.Sign(board[i + count, j].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j });
										}
										possible = false;
									}
									count++;
								}

								possible = true;
								count = -1;
								while (possible && i + count >= 0) {
									if (board[i + count, j] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i + count, j });
									} else {
										if (Math.Sign(board[i + count, j].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i + count, j });
										}
										possible = false;
									}
									count--;
								}

								possible = true;
								count = 1;
								while (possible && j + count < 8) {
									if (board[i, j + count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i, j + count });
									} else {
										if (Math.Sign(board[i, j + count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i, j + count });
										}
										possible = false;
									}
									count++;
								}

								possible = true;
								count = -1;
								while (possible && j + count >= 0) {
									if (board[i, j + count] == null) {
										legalMoves[new int[] { i, j }].Add(new int[] { i, j + count });
									} else {
										if (Math.Sign(board[i, j + count].id) != player) {
											legalMoves[new int[] { i, j }].Add(new int[] { i, j + count });
										}
										possible = false;
									}
									count--;
								}
								break;
							case 6: // King
							case -6:
								if (i < 7 && (board[i + 1, j] == null || Math.Sign(board[i + 1, j].id) != player)) {
									legalMoves[new int[] { i, j }].Add(new int[] { i + 1, j });
								}

								if (i < 7 && j < 7 && (board[i + 1, j + 1] == null || Math.Sign(board[i + 1, j + 1].id) != player)) {
									legalMoves[new int[] { i, j }].Add(new int[] { i + 1, j + 1 });
								}

								if (j < 7 && (board[i, j + 1] == null || Math.Sign(board[i, j + 1].id) != player)) {
									legalMoves[new int[] { i, j }].Add(new int[] { i, j + 1 });
								}

								if (i > 0 && j < 7 && (board[i - 1, j + 1] == null || Math.Sign(board[i - 1, j + 1].id) != player)) {
									legalMoves[new int[] { i, j }].Add(new int[] { i - 1, j + 1 });
								}

								if (i > 0 && (board[i - 1, j] == null || Math.Sign(board[i - 1, j].id) != player)) {
									legalMoves[new int[] { i, j }].Add(new int[] { i - 1, j });
								}

								if (i > 0 && j > 0 && (board[i - 1, j - 1] == null || Math.Sign(board[i - 1, j - 1].id) != player)) {
									legalMoves[new int[] { i, j }].Add(new int[] { i - 1, j - 1 });
								}

								if (j > 0 && (board[i, j - 1] == null || Math.Sign(board[i, j - 1].id) != player)) {
									legalMoves[new int[] { i, j }].Add(new int[] { i, j - 1 });
								}

								if (i < 7 && j > 0 && (board[i + 1, j - 1] == null || Math.Sign(board[i + 1, j - 1].id) != player)) {
									legalMoves[new int[] { i, j }].Add(new int[] { i + 1, j - 1 });
								}

								if (Math.Sign(board[i, j].id) == player) {
									currentKingPosition = new int[] { i, j };
								}
								break;
						}
					}
				}
			}

			if (checkKing) {
				Dictionary<int[], List<int>> movesToRemove = new Dictionary<int[], List<int>>();
				foreach (KeyValuePair<int[], List<int[]>> legalMoveSet in legalMoves) {
					int i = 0;
					foreach(int[] legalMove in legalMoveSet.Value) {
						Chessboard nextMove = DeepCopy(this);
						nextMove.stringMakeMove(moveToText(legalMoveSet.Key, legalMove), player);
						if(legalMove[0] == 6 && legalMove[1] == 4) {
							// nextMove.displayBoard(1);
						}
						Dictionary<int[], List<int[]>> tempLegalMoves = nextMove.getLegalMoves(player * -1, false);
						foreach(KeyValuePair<int[], List<int[]>> tempLegalMoveSet in tempLegalMoves) {
							foreach (int[] tempMove in tempLegalMoveSet.Value) {
								if (nextMove.board[tempMove[0], tempMove[1]] != null && nextMove.board[tempMove[0], tempMove[1]].id == player * 6) {
									// Console.WriteLine("Removed a bad move!" + legalMoveSet.Key[0] + legalMoveSet.Key[1] + "-" + legalMoveSet.Value[i][0] + legalMoveSet.Value[i][1]);
									try {
										movesToRemove[legalMoveSet.Key].Add(i);
									} catch {
										movesToRemove[legalMoveSet.Key] = new List<int>();
										movesToRemove[legalMoveSet.Key].Add(i);
									}
									// legalMoves[legalMoveSet.Key].RemoveAt(i);
									// break;
								}
							}
						}
						i++; 
					}
				}

				foreach (KeyValuePair<int[], List<int>> moveToRemove in movesToRemove) {
					List<int> alreadyRemoved = new List<int>();
					foreach (int i in moveToRemove.Value.AsEnumerable().Reverse()) {
						// Console.WriteLine("We are going to remove the move for " + moveToRemove.Key[0] + moveToRemove.Key[1] + " at index " + i);
						if (!alreadyRemoved.Contains(i)) {
							try {
								legalMoves[moveToRemove.Key].RemoveAt(i);
							} catch { }
							alreadyRemoved.Add(i);
						}
					}
				}
			}
			return legalMoves;
		}

		public static String moveToText(int[] from, int[] to) {
			return (String)("" + (char)(from[1] + 65) + (int)(from[0] + 1) + "-" + (char)(to[1] + 65) + (int)(to[0] + 1));
		}
	}

	class Program {
		public static String moveToText(int[] from, int[] to) {
			return (String)("" + (char)(from[1] + 65) + (int)(from[0] + 1) + "-" + (char)(to[1] + 65) + (int)(to[0] + 1));
		}

		static public int[][] minimax(int layers, Chessboard board, int player) {
			if (layers == 0) {
				int[][] highScoreArr = new int[2][];
				highScoreArr[0] = new int[2] { board.evaluateBoard(), 0 };
				highScoreArr[1] = new int[2] { 0, 0 };
				// Console.WriteLine("Final board score: " + highScoreArr[0][0]);
				return highScoreArr;
			} else {
				Dictionary<int[], List<int[]>> moves = board.getLegalMoves(player, true);
				int score = 0;
				bool firstRun = true;
				int highScore = 0;
				int[] bestMoveTo = { 0, 0 };
				int[] bestMoveFrom = { 0, 0 };
				foreach (KeyValuePair<int[], List<int[]>> moveSet in moves) {
					foreach (int[] move in moveSet.Value) {
						Chessboard newBoard = Chessboard.DeepCopy(board);
						newBoard.stringMakeMove(moveToText(moveSet.Key, move), player);
						if (firstRun) {
							score = minimax(layers - 1, newBoard, -player)[0][0];
							highScore = score;
							bestMoveFrom = moveSet.Key;
							bestMoveTo = move;
							firstRun = false;
						} else {
							score = minimax(layers - 1, newBoard, -player)[0][0];
						}
						if ((score > highScore && player == 1) || (score < highScore && player == -1)) {
							// Console.WriteLine("Sick move found");
							highScore = score;
							bestMoveFrom = moveSet.Key;
							bestMoveTo = move;
						}
					}
				}
				int[][] highScoreArr = new int[3][];
				highScoreArr[0] = new int[2] { highScore, 0 };
				highScoreArr[1] = bestMoveFrom;
				highScoreArr[2] = bestMoveTo;
				// if (layers == 2) { Console.WriteLine("Best move is " + bestMoveFrom[0] + ":" + bestMoveFrom[1] + " -> " + bestMoveTo[0] + ":" + bestMoveTo[1]); }
				if (layers == 2) { Console.Write("."); }
				return highScoreArr;
			}
		}

		static void Main(string[] args) {
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			Chessboard testBoard = new Chessboard();

			while (true) {
				testBoard.displayBoard(1);

				Regex validMove = new Regex("^[a-hA-H][1-8]-[a-hA-H][1-8]$");
				String lastMove = "";
				while (testBoard.stringMakeMove(lastMove, 1) == 0) {
					Console.Write("Please enter a valid move: ");
					lastMove = Console.ReadLine();
				}
				Console.WriteLine("Valid move! Score: " + testBoard.evaluateBoard());
				testBoard.displayBoard(1);

				int[][] minMaxResult = minimax(3, testBoard, 1);

				Console.WriteLine("\r\n\r\nComputer made move " + moveToText(minMaxResult[1], minMaxResult[2]) + "\r\n");
				testBoard.stringMakeMove(moveToText(minMaxResult[1], minMaxResult[2]), -1);
			}



			/*
			Console.WriteLine();
			Dictionary<int[], List<int[]>> currentLegalMoves = testBoard.getLegalMoves(-1, true);
			foreach (KeyValuePair<int[], List<int[]>> moveSet in currentLegalMoves) {
				foreach (int[] move in moveSet.Value) {
					Console.WriteLine("Legal move: " + (char)(moveSet.Key[1] + 65) + (moveSet.Key[0] + 1) + "-" + (char)(move[1] + 65) + (move[0] + 1));
					Chessboard nextMove = Chessboard.DeepCopy(testBoard);
					nextMove.stringMakeMove(Chessboard.moveToText(moveSet.Key, move), -1);
					// nextMove.displayBoard(1);
					// Console.WriteLine("Score for this board: " + nextMove.evaluateBoard());
					// Console.WriteLine();
				}
			}
			Console.WriteLine();

			Console.WriteLine(Convert.ToInt32(Console.ReadLine()));

			int[] moveToCheck = new int[] { Convert.ToInt32(Console.ReadLine()), Convert.ToInt32(Console.ReadLine()) };
			Console.WriteLine(currentLegalMoves[moveToCheck]);


			*/
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}
	}
}
