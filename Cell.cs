using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    internal struct Address(int row,int col)
    {
        public int row = row;
        public int col = col;
    }

    internal class Cell : PictureBox
    {
        /// <summary>
        /// セルの状態
        /// </summary>
        public enum Mode
        {
            ///<summary>
            ///正体_空白0
            ///</summary>
            ans_blank0 = 0,
            ///<summary>
            ///正体_空白1
            ///</summary>
            ans_blank1 = 1,
            ///<summary>
            ///正体_空白2
            ///</summary>
            ans_blank2 = 2,
            ///<summary>
            ///正体_空白3
            ///</summary>
            ans_blank3 = 3,
            ///<summary>
            ///正体_空白4
            ///</summary>
            ans_blank4 = 4,
            ///<summary>
            ///正体_空白5
            ///</summary>
            ans_blank5 = 5,
            ///<summary>
            ///正体_空白6
            ///</summary>
            ans_blank6 = 6,
            ///<summary>
            ///正体_空白7
            ///</summary>
            ans_blank7 = 7,
            ///<summary>
            ///正体_空白8
            ///</summary>
            ans_blank8 = 8,
            ///<summary>
            ///正体_爆弾
            ///</summary>
            ans_mine = 9,
            ///<summary>
            ///ボタン_爆弾処理失敗
            ///</summary>
            ans_mineX = 10,
            ///<summary>
            ///ボタン_空白
            ///</summary>
            btn_blank = 11,
            ///<summary>
            ///ボタン_旗
            ///</summary>
            btn_flag = 12,
            ///<summary>
            ///ボタン_検討
            ///</summary>
            btn_hold = 13,
            ///<summary>
            ///ボタン_旗不正
            ///</summary>
            btn_flagX = 22,
            ///<summary>
            ///ボタン_検討不正
            /// </summary>
            btn_holdX = 23,
        }

        ///<summary>
        ///正体
        ///</summary>
        public Mode Ans { get; set; }

        ///<summary>
        ///現在の状態
        /// </summary>
        private Mode _thisMode;
        public Mode ThisMode
        {
            get => _thisMode;
            set
            {
                //代入してきたモードをクラスメンバへ格納する
                _thisMode = value;
                //代入してきたモードの画像に変更する
                ChangeImg(value);
            }
        }

        ///<summary>
        ///アドレス読込専用プロパティ
        /// </summary>
        public Address Address { get; }

        ///<summary>
        ///Form1で所持しているMatrixの参照
        /// </summary>
        private readonly Cell[,] Matrix;

        ///<summary>
        ///コンストラクタ  
        /// </summary>
        /// <param name="point"></param>
        /// <param name="matrix"></param>
        public Cell(Address address, Cell[,] matrix)
        {
            //このオブジェクト
            Address = address;
            Matrix = matrix;
            ThisMode = Mode.btn_blank; //初期のセルの状態

        }

        ///<summary>
        ///ThisMode書き込みプロパティ用の画像変更メソッド
        /// </summary>
        /// <param name="mode"></param>
        private void ChangeImg(Mode mode)
        {
            //モードの種類によってセルの画像を変更する
            switch(mode)
            {
                case Mode.ans_blank0:
                    BackgroundImage = Properties.Resources.ans_blank0;
                    break;
                case Mode.ans_blank1:
                    BackgroundImage = Properties.Resources.ans_blank1;
                    break;
                case Mode.ans_blank2:
                    BackgroundImage = Properties.Resources.ans_blank2;
                    break;
                case Mode.ans_blank3:
                    BackgroundImage = Properties.Resources.ans_blank3;
                    break;
                case Mode.ans_blank4:
                    BackgroundImage = Properties.Resources.ans_blank4;
                    break;
                case Mode.ans_blank5:
                    BackgroundImage = Properties.Resources.ans_blank5;
                    break;
                case Mode.ans_blank6:
                    BackgroundImage = Properties.Resources.ans_blank6;
                    break;
                case Mode.ans_blank7:
                    BackgroundImage = Properties.Resources.ans_blank7;
                    break;
                case Mode.ans_blank8:
                    BackgroundImage = Properties.Resources.ans_blank8;
                    break;
                case Mode.ans_mine:
                    BackgroundImage = Properties.Resources.ans_mine;
                    break;
                case Mode.ans_mineX:
                    BackgroundImage = Properties.Resources.ans_mineX;
                    break;
                case Mode.btn_blank:
                    BackgroundImage = Properties.Resources.btn_blank;
                    break;
                case Mode.btn_flag:
                    BackgroundImage = Properties.Resources.btn_flag;
                    break;
                case Mode.btn_hold:
                    BackgroundImage = Properties.Resources.btn_hold;
                    break;
                case Mode.btn_flagX:
                    BackgroundImage = Properties.Resources.btn_flagX;
                    break;
                case Mode.btn_holdX:
                    BackgroundImage = Properties.Resources.btn_holdX;
                    break;
            }
        }

        /// <summary>
        /// 自身の状態が空白ボタンの時、正体を明かす
        /// もし正体が空白0なら、周りの正体も明かす 
        /// coercion(強制)がTrueの時は、空白ボタンで無くても
        /// 強制的に正体を明かす
        /// </summary>
        /// <param name="coercion">強制フラグ。True時は、強制的に正体を明かす。</param> 
        public void DisplayAns(bool coercion = false)
        {
            // 自身の状態が空白ボタンでない時は、何もしない
            // 但し、強制フラグが立っているときはスルーする
            if (!coercion && _thisMode != Mode.btn_blank)
            {
                return;
            }

            //ここでThisModeのセッターが動作し、対応するモードの画像に変わる
            ThisMode = Ans;

            //もし開けた結果が正体_空白0の時は、周りの8箇所も開ける
            if(Ans == Mode.ans_blank0)
            {
                for(int row = -1; row <= 1; row++ )
                {
                    for(int col = -1; col <= 1; col++ )
                    {
                        //もしこのセルの位置ではなく、
                        //かつ、rowがマイナスでもなくトータルの行数を超えておらず、
                        //かつ、colがマイナスでもなくトータルの列数を超えていない時、
                        if(!(row == 1 && col == 1) &&
                            Address.row + row >= 0 &&
                            Address.row + row <= Properties.Settings.Default.NumberOfRows - 1 &&
                            Address.col + col >= 0 &&
                            Address.col + col <= Properties.Settings.Default.NumberOfColumns - 1)
                        {
                            // 周りのセルの同メソッドを実行する
                            Matrix[Address.row + row, Address.col + col].DisplayAns();
                        }
                    }
                }
            }
        }

        ///<summary>
        ///ボタンを右クリックした時のボタン変更処理
        /// </summary>
        public void ChangeBtnMode()
        {
            // もしボタンでなければ何もしない
            if ((int)ThisMode <= 9)
            {
                return;
            }

            // 空白⇒旗⇒検討…の順に切り替わるようにする
            int modeNum = (int)ThisMode + 1;
            if(modeNum > 13)
            {
                modeNum = 11;
            }

            //ここでThismodeのセッターが動作し、対応するモードの画像に変わる
            //数値型からMode列挙体へ型変換し代入する
            ThisMode = (Mode)Enum.ToObject(typeof(Mode), modeNum);
        }

        ///<summary>
        /// 自身の正体が空白セルのとき、自身の周り8マスの爆弾の総数を数えて、
        /// 自身の正体にナンバリングする。
        /// 但し、自身の周り全てが爆弾や壁で囲まれている場合は、
        /// 自身を空白セルとし、ナンバリングを行う。
        ///</summary>
        public void RefreshAns()
        {
            // 周り８か所の爆弾数
            int mineCounter = 0;
            // 周り8か所の端数
            int edgeCounter = 0;
            for (int row = -1; row <= 1; row++)
            {
                for(int col = -1;col <= 1; col++ ) 
                {
                    // 周りの座標の時
                    if(!(row == 0 && col == 0))
                    {
                        // 枠外ではない時
                        if(Address.row + row >= 0 &&
                           Address.row + row <= Properties.Settings.Default.NumberOfRows - 1 &&
                           Address.col + col >= 0 &&
                           Address.col + col <= Properties.Settings.Default.NumberOfColumns - 1)
                        {
                            // 座標のセルが正体_爆弾の時
                            if (Matrix[Address.row + row, Address.col + col].Ans == Mode.ans_mine)
                            {
                                // 爆弾数をインクリメントする
                                mineCounter += 1;
                            }
                        }
                        // 枠外の時
                        else
                        {
                            // 端数をインクリメントする
                            edgeCounter += 1;
                        }
                    }
                }
            }

            // このセルの正体が爆弾でないとき
            if(Ans != Mode.ans_mine)
            {
                // 正体が空白ということなのでナンバリングする
                Ans = (Mode)mineCounter;
            }
            // このセルの正体が爆弾の時
            else
            {
                // 周り全てが爆弾か端っこのときはこのセルの正体を爆弾ではなく空白とし、
                // ナンバリングする(※基本設計の正体欄を参照)
                if(mineCounter + edgeCounter == 8)
                {
                    // 周りが壁か3つ以上の爆弾しかパターンがないので、ナンバリングする
                    Ans = (Mode)mineCounter;
                }
            } 
        }

        /// <summary>
        /// このセルがユーザーによって暴かれているか判断する
        /// </summary>
        /// <returns>正体が暴かれているばTrueを、暴かれていなければFalseを返します。</returns>
        public Boolean CheckDisplayAns()
        {
            //正体が爆弾で、旗を立てていればTrueを返す
            if(Ans == Mode.ans_mine && ThisMode == Mode.btn_flag)
            {
                return true;
            }
           // 空白セルをめくり済みであればTrueを、それ以外であればFalseを返す
            return (int) ThisMode < 9; 
        }
    }
}
