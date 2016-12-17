C# Layered Window 생성 DLL
개발 툴 : Microsoft Visual Studio 15
대상 프레임워크 : .NET Framework 4 Client Profile

1. 설명
 32비트 투명 배경을 가지는 png 파일을 이용하여 Layerd Form을 그려준다.
 MSDN의 예제파일을 사용하기 쉽도록 dll화 한 프로젝트 이다.
 System.Windows.Forms.Form class를 상속받아 LayeredForm class를 구현 했다.

2. 내장 Override 메서드
  1) CreateParams : LAYERED 스타일 반영
  2) OnMouseMove,OnMouseDown : Form의 위치 이동 지원
  3) OnMouseDoubleClick : Form 종료

3. 사용 예제
  1. Windows Forms Application Project 생성
  2. 기본 생성된 Form class를 InitForm으로 정정
  3. 실제 LayerdForm이 될 MainForm 생성
    1) System.Windows.Forms.Form이 아닌 LayeredForm을 상속 받는다.
    2) Load 이벤트를 등록 후 LayeredForm의 protected 메서드인 SelectBitmap를 호출 한다.
  4. InitForm에 Load 이벤트를 등록 후 MainForm을 생성 한다.

    // InitForm 소스 코드
    public partial class InitForm : Form {
        public InitForm() {
            InitializeComponent();

            // InitForm 숨김
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;

            // MainForm 표기
            MainForm form = new MainForm();
            form.FormClosing += InitFormClosing;
            form.Show();
        }

        // MainForm 종료시 기본 Form인 InitForm을 종료
        private void InitFormClosing(object sender, FormClosingEventArgs e) {
            Close();
        }
    }

    // MainForm 소스 코드
    public partial class MainForm : LayeredForm {
        public MainForm() {
            InitializeComponent();

	    // Title 지정
            setCaption("MainForm");
        }

        private void MainForm_Load(object sender, EventArgs e) {
            SelectBitmap(Properties.Resources.BackgroundImg.png);
        }
    }