using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security.Cryptography.Pkcs;
namespace Minesweeper
{
    public partial class Form1 : Form
    {
        // �S�ẴZ�����i�[���Ă������߂ɁA�񎟌��z���p�ӂ���B
        private readonly Cell[,] _matrix = new Cell[
                                    Properties.Settings.Default.NumberOfRows,
                                    Properties.Settings.Default.NumberOfColumns];
        // cell�A�C�R���̈�ӂ̃T�C�Y
        private const int CellSize = 25;

        /// <summary>
        /// �Q�[���I�����̎��
        ///  </summary>
        enum EndType
        {
            /// <summary>
            /// �N���A
            /// </summary>
            gameClear,
            /// <summary>
            /// ���s
            /// </summary>
            gameOver,
        }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public Form1()
        {
            // ���́uInitializeComponent�v���\�b�h�́AVisual Studio�ɂ��
            // �����������ꂽ���̂Ȃ̂ŐG��Ȃ����ƁB
            // ��ɁA�f�U�C����G�����Ƃ���A�C�x���g��ǉ���������
            // ���\�b�h�̓��e�������ōX�V����܂��B
            InitializeComponent();
            // �R���X�g���N�^�ɏ������������́uInitializeComponent�v����
            // ���ɋL�q���܂��傤�B
        }

        /// <summary> 
        /// �t�H�[����ǂݍ��ގ��ɔ�������C�x���g�n���h��
        /// - Cell��z�u����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // 1�`�s���̐ݒ萔�������A���[�v��������
            for (int row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                // 1�`�񐔂̐ݒ萔�������A���[�v��������
                for (int col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    // �Ώ�cell�̃A�h���X(row, col)
                    Address address = new Address(row, col);

                    // cell�I�u�W�F�N�g���C���X�^���X��
                    Cell cell = new Cell(address, _matrix);

                    // Cell�̓s�N�`���[�{�b�N�X�R���g���[�����p�����Ă���̂ŁA
                    // �p�l����t�H�[���̃R���g���[�����ɒu�����ƂŁA
                    // �`�揈���������ōs���Ă����悤�ɂȂ�B
                    // ����̓p�l���ɕ��ׂĕ\���������̂ňȉ��̂悤�ɂ����B
                    panel1.Controls.Add(cell);

                    // Form1�����o�[�ɗp�ӂ���_matrix�֐��������Z����ݒ肷��B
                    _matrix[row, col] = cell;

                    // �Z���̃v���p�e�B��K�؂ɕύX����
                    cell.BackgroundImageLayout = ImageLayout.Stretch;
                    cell.Location = new Point(col * CellSize, row * CellSize);
                    cell.Name = $"Cell({row},{col})";// �f�o�b�O���ɃI�u�W�F�N�g��������W���킩��ƕ֗��Ȃ̂ŁA�ύX���Ă���
                    cell.Size = new Size(CellSize, CellSize);
                    cell.BackgroundImage = Properties.Resources.btn_blank; // �󔒂̃{�^����\������
                    cell.Enabled = false; // �X�^�[�g����܂Ń{�^���������ł��Ȃ��悤�ɂ���

                    // MouseDown�C�x���g�ɁA�n���h��(���s���鏈��)��ǉ�����
                    // MouseEventHandler�Ƃ������̃f���Q�[�g����āACellClick���\�b�h��ǉ����Ă��܂�
                    // CellClick�̌��()���Ȃ����Ƃɗ��ӂ��Ă��������B
                    // CellClick���\�b�h�����s���Ă���̂ł͂Ȃ��A���\�b�h���̂�n���Ă���̂ł��B
                    cell.MouseDown += new MouseEventHandler(CellClick);
                }

            }

            //cell�̔z�u���I�������Q�[���X�^�[�g
            GameStart();
        }

        /// <summary>
        /// �Z�����N���b�N�������̃C�x���g�n���h��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellClick(object? sender, MouseEventArgs e) // <- Object
        {
            // �ϐ��̐錾���A�������l�̃f�[�^�^�����m�ȏꍇ�́Avar�𗘗p�ł��܂��B
            var Cell = (Cell?)sender; // <- var cell = (cell)sender
            if (Cell == null) return; // �ǉ�

            // �}�E�X�̂ǂ̃{�^���������ꂽ���ɂ���āA�����𕪂���
            switch (e.Button)
            {
                case MouseButtons.Left:
                    // ���N���b�N��
                    CellLeftClick(Cell);
                    break;
                case MouseButtons.Right:
                    // �E�N���b�N��
                    CellRightClick(Cell);
                    break;
            }
        }

        /// <summary>
        /// �Z�������N���b�N���̏���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellLeftClick(Cell cell)
        {
            // ���̂�\������
            cell.DisplayAns();
            // �������N���b�N�����Z�������̔��e�̎�
            if (cell.ThisMode == Cell.Mode.ans_mine)
            {
                // �Q�[���I�[�o�[�̏��������s����
                GameEnd(EndType.gameOver, cell);
            }
            // ���e�ȊO�̎�
            else
            {
                // �Q�[���N���A���`�F�b�N����
                if (GameClearCheck())
                {
                    // �N���A�̎��̓N���A�̏��������s����
                    GameEnd(EndType.gameClear);
                }
            }
            // �Q�[���I�[�o�[�ł��Q�[���N���A�ł��Ȃ��ꍇ�͂��̏����𔲂��܂�
        }

        /// <summary>
        /// �Z�����E�N���b�N�������̏���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellRightClick(Cell cell)
        {
            // �Z�����{�^���̎��A�󔒁ˊ��ˌ����̏��ɐ؂�ւ���
            cell.ChangeBtnMode();
            // �Q�[���N���A���`�F�b�N����
            if (GameClearCheck())
            {
                // �N���A�̎��̓N���A�̏��������s����
                GameEnd(EndType.gameClear);
            }
        }

        ///<summary>
        /// �X�^�[�g�{�^���������̏���
        /// ���e�̔z�u�����������A�󔒃Z���ɐ���������U��
        /// </summary>
        private void GameStart()
        {
            // �����_���Ȓl���擾���邽�߂Ƀ����_���I�u�W�F�N�g�𐶐�����
            var mineRandom = new Random();

            // �S�Z���Ƀ����_���ɔ��e��z�u����
            for (var row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                for (var col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    // 100�܂ł̃����_���Ȓl���擾
                    var mineNum = mineRandom.Next(100);

                    // �Z���̐��̂��i�[����ϐ���錾���A�����l�͋󔒂ɂ���
                    var ans = Cell.Mode.ans_blank0;

                    // �����_���Ȓl�����e�����ȉ��̎��͐��̂𔚒e�ɐݒ肷��
                    if (mineNum <= Properties.Settings.Default.MineRate)
                    {
                        ans = Cell.Mode.ans_mine;
                    }
                    _matrix[row, col].ThisMode = Cell.Mode.btn_blank; // �󔒃{�^���̏�Ԃɂ���
                    _matrix[row, col].Ans = ans; // �����ŁA���̂ɐ��̔��e�����̋󔒂������邱�ƂɂȂ�]
                    _matrix[row, col].Enabled = true; // �N���b�N�ł���悤�Ɋ���������

                }
            }

            // ���̂��󔒃Z���Ƀi���o�����O���s��
            // ���肪�S�ĕǂ����e�̃Z���͋󔒃Z���ɂ��A�i���o�����O���s���B
            for (var row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                for (var col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    _matrix[row, col].RefreshAns();
                }
            }
        }
        ///<summary>
        /// �Q�[���I�[�o�[�̎��́A���ׂĂ̔��e�̈ʒu���N���A
        /// ���̂����e�̂Ƃ���Ɋ��⌟���̃{�^�����������
        /// �s���}�[�N(X)��t����
        /// ���̑��A�����I�����s��
        /// </summary>
        /// <param name="endType"�Q�[���N���A���Q�[���I�[�o�[</param>
        /// <param name="cell">�Z���B�Q�[���I�[�o�[���̂ݎw�肷��</param>
        private void GameEnd(EndType endType, Cell? cell = null)
        {
            // �S�ẴZ���ɑ΂���
            for (var row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                for (var col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    // �Z�����N���b�N�o���Ȃ��悤�ɂ���
                    _matrix[row, col].Enabled = false;

                    // �Q�[���I�[�o�[�̎���
                    if (endType == EndType.gameOver)
                    {
                        // ���̂����e�̎�
                        if (_matrix[row, col].Ans == Cell.Mode.ans_mine)
                        {
                            //�Z���𖾂���
                            _matrix[row, col].DisplayAns(true);
                        }
                    }
                    //���̂����e�ȊO�̎�
                    else
                    {
                        //�������⌟�������Ă�����A���[�U�[�̗\�z���O�ꂽ�Ƃ������ƂȂ̂ŁA
                        //�s���}�[�N(X)��t����
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


            // �Q�[���N���A���Q�[���I�[�o�[�ɂɕ����ă��b�Z�[�W���o�͂���
            switch (endType)
            {
                case EndType.gameClear:
                    MessageBox.Show("�Q�[���N���A�I�I\r\n���߂łƂ��������܂��I�I",
                        "GameClear", MessageBoxButtons.OK, MessageBoxIcon.None);
                    break;
                case EndType.gameOver:
                    // ���N���b�N���Ă��܂������e��X�}�[�N��t����
                    if (cell != null) cell.ThisMode = Cell.Mode.ans_mineX;
                    MessageBox.Show("�Q�[���I�[�o�[�B�B\r\n�c�O�Ȃ��甚�e�̏����Ɏ��s���܂���",
                        "GameOver", MessageBoxButtons.OK, MessageBoxIcon.None);
                    break;
            }
        }

        /// <summary>
        /// �Q�[�����N���A���Ă��邩�`�F�b�N����
        /// </summary>
        /// <return>�N���A�Ȃ�true�A����ȊO�Ȃ�false��Ԃ�</return>
        private Boolean GameClearCheck()
        {
            for (var row = 0; row < Properties.Settings.Default.NumberOfRows; row++)
            {
                for (var col = 0; col < Properties.Settings.Default.NumberOfColumns; col++)
                {
                    // ��ł��\����Ă��Ȃ��Z��������΁A��False��Ԃ��Ĕ�����
                    if (!_matrix[row, col].CheckDisplayAns())
                    {
                        return false;
                    }
                }
            }

            // �����܂ŏ����������Ƃ������Ƃ́A���ׂẴZ�����\����Ă���Ƃ������ƂȂ̂ŁA
            // True��Ԃ�
            return true;
        }

        /// <summary>
        /// �X�^�[�g�{�^���N���b�N�C�x���g�n���h��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_start_Click(object sender, EventArgs e)
        {
            GameStart();
        }
    }
}
 