using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace inm_mov_mimicopy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ANSWERFILENAME = "answer.txt";

        Random _rnd     = new();

        int _crtNumber = 0;

        List<string> _movList = [];
        List<int>    _unusedNumberList = [];

        List<Tuple<int, string>> _answers = [];


        public MainWindow()
        {
            InitializeComponent();

            // 回答ファイルの存在確認。上書きが駄目なら終了。
            if (File.Exists( ANSWERFILENAME ))
            {
                if( MessageBoxResult.Cancel == MessageBox.Show( "回答ファイルを上書きしてもいいのか？", "確認", MessageBoxButton.OKCancel ))
                {
                    Close();
                    return;
                }
            }

            // 動画の一覧を作成。
            InitMovieList();

            // 最初の動画を設定。
            if (false == PlayNextVideo())
            {
                Close();
            }
        }




        // 動画リストを初期化。
        private void InitMovieList()
        {
            try
            {
                string folderName;
                var    clArgs = Environment.GetCommandLineArgs();

                // 実行ファイルのパスの次にディレクトリ パスが渡されているかもしれないので、それを使う。
                if ( 1 < clArgs.Length )
                {
                    folderName = clArgs[1];
                }
                else
                {
                    // フォルダを選択。選択されたら動画の一覧を作成。
                    OpenFolderDialog ofd = new();
                    ofd.Title = "Select INM Folder";
                    if (true != ofd.ShowDialog())
                    {
                        Close();
                        return;
                    }
                    folderName = ofd.FolderName;
                }

                // パス一覧をリストに保存。
                _movList          = Directory.GetFiles( folderName ).ToList();
                _unusedNumberList = Enumerable.Range(0, _movList.Count).ToList();

            }
            catch ( Exception )
            {
                MessageBox.Show( "ERR00: Failed to load the file list!" );
                Close();
            }
        }




        // 次の動画に移行。
        public bool PlayNextVideo()
        {
            // フォルダ内の動画をランダムで再生。
            int idx    = _rnd.Next( _unusedNumberList.Count );
            int number = _unusedNumberList[idx];
            if (SetVideoToPlayer( number ))
            {
                // 動画番号を設定する。選択された動画はもう登場しないのでリストから削除。
                _crtNumber = number;
                _unusedNumberList.RemoveAt( idx );
                return true;
            }
            return false;
        }




        // 動画をプレーヤに設定。
        private bool SetVideoToPlayer(
            int number
        )
        {
            try
            {
                media_player.Source = new Uri( _movList[number] );
            }
            catch (Exception)
            {
                MessageBox.Show( "ERR01: Failed to load resource! : " + _movList[number] );
                return false;
            }
            return true;
        }




        // 回答を保存。
        private void SaveAnswers()
        {
            try
            {
                // ファイルオープン。
                using (StreamWriter sw = new( ANSWERFILENAME ))
                {
                    // 動画リストを書き込み。
                    for (int i = 0; i < _movList.Count; ++i)
                    {
                        sw.WriteLine( i + ": " + _movList[i] );
                    }
                    // 空行を挿入。
                    sw.WriteLine( "" );
                    // 回答を書き込み。
                    if ( null != _answers )
                    {
                        foreach ( var itm in _answers )
                        {
                            sw.WriteLine( itm );
                        }
                    }
                }
            }
            catch
            {
                // 書き込み失敗したら再試行するか確認。
                if (MessageBoxResult.Yes == MessageBox.Show( "回答ファイルの書き込み失敗。再試行？", "確認", MessageBoxButton.YesNo ))
                {
                    SaveAnswers();
                }
            }
        }








        // 次ボタンを押したとき。
        private void btn_next_Click( object sender, RoutedEventArgs e )
        {
            // 回答を保存しテキストボックスをクリア。
            _answers.Add( new( _crtNumber, txb_answer.Text ) );
            txb_answer.Text = "";
            txb_answer.Focus();

            // まだ動画があるか。
            if (0 < _unusedNumberList.Count)
            {
                // 次の動画を再生。
                if (false == PlayNextVideo())
                {
                    Close();
                }
            }
            else
            {
                // 回答を保存し終了。
                SaveAnswers();
                Close();
            }
        }

        private void txb_answer_KeyDown( object sender, KeyEventArgs e )
        {
            // エンターキーで確定。
            if ( e.Key == Key.Enter)
            {
                (new ButtonAutomationPeer( btn_next ) as IInvokeProvider).Invoke();
            }
        }
    }
}