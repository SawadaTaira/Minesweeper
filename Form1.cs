using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security.Cryptography.Pkcs;
namespace Minesweeper
{
    public partial class Form1 : Form
    {
        // 全てのセルを格納しておくために、二次元配列を用意する。
        private readonly Cell[,] _matrix = new Cell[
                                    Properties.Settings.Default.NumberOfRows,
                                    Properties.Settings.Default.NumberOfColumns];
        // cellアイコンの一辺のサイズ
        private const int CellSize = 25;

        /// <summary>
        /// ゲーム終了時の種類
        ///  </summary>
        enum EndType
        {
            /// <summary>
            /// クリア
            /// </summary>
            gameClear,
            /// <summary>
            /// 失敗
            /// </summary>
            gameOver,
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Form1()
        {
            // この「InitializeComponent」メソッドは、Visual Studioにより
            // 自動生成されたものなので触らないこと。
            // 主に、デザインを触ったときや、イベントを追加した時に
            // メソッドの内容が自動で更新されます。
            InitializeComponent();
            // コンストラクタに書きたい処理は「InitializeComponent」よりも
            // 下に記述しましょう。
        }

        /// <summary> 
        /// フォームを読み込む時に発生するイベントハンドラ
        /// - Cellを配置する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // 1～行数の設定数分だけ、ループ処理する
            for (int row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                // 1～列数の設定数分だけ、ループ処理する
                for (int col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    // 対象cellのアドレス(row, col)
                    Address address = new Address(row, col);

                    // cellオブジェクトをインスタンス化
                    Cell cell = new Cell(address, _matrix);

                    // Cellはピクチャーボックスコントロールを継承しているので、
                    // パネルやフォームのコントロール下に置くことで、
                    // 描画処理を自動で行ってくれるようになる。
                    // 今回はパネルに並べて表示したいので以下のようにした。
                    panel1.Controls.Add(cell);

                    // Form1メンバーに用意した_matrixへ生成したセルを設定する。
                    _matrix[row, col] = cell;

                    // セルのプロパティを適切に変更する
                    cell.BackgroundImageLayout = ImageLayout.Stretch;
                    cell.Location = new Point(col * CellSize, row * CellSize);
                    cell.Name = $"Cell({row},{col})";// デバッグ時にオブジェクト名から座標がわかると便利なので、変更しておく
                    cell.Size = new Size(CellSize, CellSize);
                    cell.BackgroundImage = Properties.Resources.btn_blank; // 空白のボタンを表示する
                    cell.Enabled = false; // スタートするまでボタンが押下できないようにする

                    // MouseDownイベントに、ハンドラ(実行する処理)を追加する
                    // MouseEventHandlerという名のデリゲートを介して、CellClickメソッドを追加しています
                    // CellClickの後に()がないことに留意してください。
                    // CellClickメソッドを実行しているのではなく、メソッド自体を渡しているのです。
                    cell.MouseDown += new MouseEventHandler(CellClick);
                }

            }

            //cellの配置が終わったらゲームスタート
            GameStart();
        }

        /// <summary>
        /// セルをクリックした時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellClick(object? sender, MouseEventArgs e) // <- Object
        {
            // 変数の宣言時、代入する値のデータ型が明確な場合は、varを利用できます。
            var Cell = (Cell?)sender; // <- var cell = (cell)sender
            if (Cell == null) return; // 追加

            // マウスのどのボタンが押されたかによって、処理を分ける
            switch (e.Button)
            {
                case MouseButtons.Left:
                    // 左クリック時
                    CellLeftClick(Cell);
                    break;
                case MouseButtons.Right:
                    // 右クリック時
                    CellRightClick(Cell);
                    break;
            }
        }

        /// <summary>
        /// セルを左クリック時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellLeftClick(Cell cell)
        {
            // 正体を表示する
            cell.DisplayAns();
            // もし左クリックしたセルが正体爆弾の時
            if (cell.ThisMode == Cell.Mode.ans_mine)
            {
                // ゲームオーバーの処理を実行する
                GameEnd(EndType.gameOver, cell);
            }
            // 爆弾以外の時
            else
            {
                // ゲームクリアかチェックする
                if (GameClearCheck())
                {
                    // クリアの時はクリアの処理を実行する
                    GameEnd(EndType.gameClear);
                }
            }
            // ゲームオーバーでもゲームクリアでもない場合はこの処理を抜けます
        }

        /// <summary>
        /// セルを右クリックした時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellRightClick(Cell cell)
        {
            // セルがボタンの時、空白⇒旗⇒検討の順に切り替える
            cell.ChangeBtnMode();
            // ゲームクリアかチェックする
            if (GameClearCheck())
            {
                // クリアの時はクリアの処理を実行する
                GameEnd(EndType.gameClear);
            }
        }

        ///<summary>
        /// スタートボタン押下時の処理
        /// 爆弾の配置を初期化し、空白セルに数字を割り振る
        /// </summary>
        private void GameStart()
        {
            // ランダムな値を取得するためにランダムオブジェクトを生成する
            var mineRandom = new Random();

            // 全セルにランダムに爆弾を配置する
            for (var row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                for (var col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    // 100までのランダムな値を取得
                    var mineNum = mineRandom.Next(100);

                    // セルの正体を格納する変数を宣言し、初期値は空白にする
                    var ans = Cell.Mode.ans_blank0;

                    // ランダムな値が爆弾割合以下の時は正体を爆弾に設定する
                    if (mineNum <= Properties.Settings.Default.MineRate)
                    {
                        ans = Cell.Mode.ans_mine;
                    }
                    _matrix[row, col].ThisMode = Cell.Mode.btn_blank; // 空白ボタンの状態にする
                    _matrix[row, col].Ans = ans; // ここで、正体に正体爆弾か正体空白かが入ることになる]
                    _matrix[row, col].Enabled = true; // クリックできるように活性化する

                }
            }

            // 正体が空白セルにナンバリングを行い
            // 周りが全て壁か爆弾のセルは空白セルにし、ナンバリングを行う。
            for (var row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                for (var col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    _matrix[row, col].RefreshAns();
                }
            }
        }
        ///<summary>
        /// ゲームオーバーの時は、すべての爆弾の位置を晒し、
        /// 正体が爆弾のところに旗や検討のボタンがある個所は
        /// 不正マーク(X)を付ける
        /// その他、処理終了を行う
        /// </summary>
        /// <param name="endType"ゲームクリアかゲームオーバー</param>
        /// <param name="cell">セル。ゲームオーバー時のみ指定する</param>
        private void GameEnd(EndType endType, Cell? cell = null)
        {
            // 全てのセルに対して
            for (var row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                for (var col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    // セルがクリック出来ないようにする
                    _matrix[row, col].Enabled = false;

                    // ゲームオーバーの時は
                    if (endType == EndType.gameOver)
                    {
                        // 正体が爆弾の時
                        if (_matrix[row, col].Ans == Cell.Mode.ans_mine)
                        {
                            //セルを明かす
                            _matrix[row, col].DisplayAns(true);
                        }
                    }
                    //正体が爆弾以外の時
                    else
                    {
                        //もし旗や検討がついていたら、ユーザーの予想が外れたということなので、
                        //不正マーク(X)を付ける
                        switch (_matrix[row, col].ThisMode)
                        {
                            case Cell.Mode.btn_flag:
                                _matrix[row, col].ThisMode = Cell.Mode.btn_flagX;
                                break;
                            case Cell.Mode.btn_hold:
                                _matrix[row, col].ThisMode = Cell.Mode.btn_holdX;
                                break;
                        }
                    }
                }
            }


            // ゲームクリアかゲームオーバーにに併せてメッセージを出力する
            switch (endType)
            {
                case EndType.gameClear:
                    MessageBox.Show("ゲームクリア！！\r\nおめでとうございます！！",
                        "GameClear", MessageBoxButtons.OK, MessageBoxIcon.None);
                    break;
                case EndType.gameOver:
                    // 左クリックしてしまった爆弾にXマークを付ける
                    if (cell != null) cell.ThisMode = Cell.Mode.ans_mineX;
                    MessageBox.Show("ゲームオーバー。。\r\n残念ながら爆弾の処理に失敗しました",
                        "GameOver", MessageBoxButtons.OK, MessageBoxIcon.None);
                    break;
            }
        }

        /// <summary>
        /// ゲームがクリアしているかチェックする
        /// </summary>
        /// <return>クリアならtrue、それ以外ならfalseを返す</return>
        private Boolean GameClearCheck()
        {
            for (var row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                for (var col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    // 一つでも暴かれていないセルがあれば、即Falseを返して抜ける
                    if (!_matrix[row, col].CheckDisplayAns())
                    {
                        return false;
                    }
                }
            }

            // ここまで処理が来たということは、すべてのセルが暴かれているということなので、
            // Trueを返す
            return true;
        }

        /// <summary>
        /// スタートボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_start_Click(object sender, EventArgs e)
        {
            GameStart();
        }
    }
}
 