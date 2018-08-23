using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace lotali_img
{
    public partial class Form1 : Form
    {
        //파일 입출력
        FileStream now_f;           //입출력변수
        StreamWriter now_writer;    //입출력변수
        List<double> histo;         //히스토그램
        
        Bitmap IMAGE;               //이미지 변수
        Color color;                //도트 색상
        bmp_control bc;             //비트맵 컨트롤
        List<int>[] list;           //교차점

        int now_select = 0;         //콤보박스 선택지

        public Form1()
        {
            InitializeComponent();
            
            histo = new List<double>(); //히스토그램

            bc = new bmp_control();     //비트맵 컨트롤
            list = new List<int>[2];    //교차점
            list[0] = new List<int>();  //x축
            list[1] = new List<int>();  //y축

            //콤보박스 입력
            comboBox1.Items.Add("0_월");
            comboBox1.Items.Add("1_일");
            comboBox1.Items.Add("2_요일");
            comboBox1.Items.Add("3_도착지");
            comboBox1.Items.Add("4_차량번호");
            comboBox1.Items.Add("5_이름");
            comboBox1.Items.Add("6_금액");
            comboBox1.Items.Add("7_메모");

            //웹 로드
            webBrowser1.Navigate(new Uri("http://xn--2o2b2r1h88c394afulx2h.com/home/index.php?ids=qwq6735&pws=dbswornjs1"));
        }
        //특징 생성
        public void gen(Bitmap bmp)
        {
            histo.Clear(); //히스토그램 초기화
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                {
                    color = bmp.GetPixel(i, j);
                    if (color.R <= 200) histo.Add(1);
                    else histo.Add(0);
                }
        }
        
        //특징데이터 생성/저장
        public void gen_f(Bitmap bmp, String label, String filename, FileMode filemode)
        {
            //학습데이터파일열기
            now_f = new FileStream(filename, filemode, FileAccess.Write);
            now_writer = new StreamWriter(now_f, System.Text.Encoding.Unicode);

            gen(bmp);           //특징 생성
            WriteData(label);     //히스토그램 저장

            //학습데이터파일닫기
            now_writer.Close();
            now_f.Close();
        }

        //특징데이터 저장
        public void WriteData(String label)
        {
            now_writer.Write(label);

            for (int i = 0; i < histo.Count; i++)
                now_writer.Write(" " + (i+1).ToString()+ ":" + histo[i]);
            now_writer.WriteLine("");
        }


        //테스트버튼
        private void button3_Click(object sender, EventArgs e)
        {
            //이미지 열기
            OpenFileDialog OPEN = new OpenFileDialog();
            OPEN.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            //이미지를 선택하면
            if (OPEN.ShowDialog() == DialogResult.OK)
            {
                IMAGE = new Bitmap(OPEN.FileName);      //비트맵 열기
                gen(IMAGE); //특징데이터 생성(histo 리스트 갱신)
                gen_f(IMAGE, "0", "tmp.txt", FileMode.Create);
            }
            switch (now_select)
            {
                case 0:
                case 1:
                    label2.Text = Program.SVM_Classification(Program.model_0).ToString(); //인식결과(월, 일)
                    break;
                case 2:
                    label2.Text = Program.SVM_Classification(Program.model_2).ToString(); //인식결과(요일)
                    break;
                case 3:
                    label2.Text = Program.SVM_Classification(Program.model_3).ToString(); //인식결과(목적지)
                    break;
                case 4:
                    label2.Text = Program.SVM_Classification(Program.model_4).ToString(); //인식결과(차량번호)
                    break;
                case 6:
                    label2.Text = Program.SVM_Classification(Program.model_6).ToString(); //인식결과(금액)
                    break;
                case 7:
                    label2.Text = Program.SVM_Classification(Program.model_7).ToString(); //인식결과(메모)
                    break;
                default:
                    break;
            }
        }
        //학습데이터 일괄처리
        private void button4_Click(object sender, EventArgs e)
        {
            //이미지 열기
            OpenFileDialog OPEN = new OpenFileDialog();
            OPEN.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            //이미지를 선택하면
            if (OPEN.ShowDialog() == DialogResult.OK)
            {
                IMAGE = new Bitmap(OPEN.FileName);  //비트맵 열기
                IMAGE = bc.bmp_find_rotate(IMAGE);  //수평선에 맞게 회전하고
                list = bc.bmp_dot(IMAGE);           //교차점 알아내고

                IMAGE.Save(".\\test.png");

                Bitmap bm = new Bitmap(IMAGE);      //이미지를 복사하고(원본이미지를 보호)
                bm = bc.bmp_binary(bm);             //이진화

                int[] blank;        //공백 좌표 xl = 0, yt = 0, xr = 0, yb = 0;

                int x_lt, y_lt;     //왼쪽위 좌표
                int x_rb, y_rb;     //오른쪽아래 좌표
                
                for (int i = 0; i < int.Parse(textBox3.Text); i++)
                {
                    x_lt = list[0][i] + 5;          //x_lt축 점으로부터 공백 5
                    x_rb = list[0][i + 1] - 5;      //x_rb축 점으로부터 공백 5
                    for (int j = 0; j < 32; j++)
                    {
                        y_lt = list[1][j] + 5;      //y_lt축 점으로부터 공백 5
                        y_rb = list[1][j + 1] - 5;  //y_rb축 점으로부터 공백 5

                        blank = bc.bmp_blank(IMAGE, x_lt, y_lt, x_rb, y_rb);    //공백찾고

                        //여기서 xl과 yt는 왼쪽위 x와 y좌표
                        //여기서 xr과 yb는 오른쪽아래 x와 y좌표

                        Bitmap cut = bc.bmp_cut(bm, blank[0], blank[1], blank[2], blank[3]); //찾은 좌표대로 자르고
                        cut = bc.bmp_resize(cut, int.Parse(widths.Text), int.Parse(heights.Text));   //(20x20)으로 리사이즈
                        cut = bc.bmp_rotate2(cut, float.Parse(textBox2.Text));  //회전
                        bc.bmp_binary(cut);                                     //이진화
                        cut.Save(".\\img\\img_"+ i.ToString() + "_" + j.ToString() + ".png");

                        int labels = i + (int.Parse(textBox4.Text));    //라벨설정
                        gen_f(cut, labels.ToString(), "wine"+ now_select.ToString() +".txt", FileMode.Append);  //학습용 특징데이터 생성
                        
                        for (int x = blank[0]-1; x <= blank[2]+1; x++)
                        {
                            IMAGE.SetPixel(x, blank[1]-1, Color.Red);
                            IMAGE.SetPixel(x, blank[3]+1, Color.Red);
                        }
                        for (int y = blank[1]-1; y <= blank[3]+1; y++)
                        {
                            IMAGE.SetPixel(blank[0]-1, y, Color.Red);
                            IMAGE.SetPixel(blank[2]+1, y, Color.Red);
                        }
                    }
                }

                IMAGE.Save(".\\test.png");
            }
        }
        //장부입력
        private void button5_Click(object sender, EventArgs e)
        {
            //이미지 열기
            OpenFileDialog OPEN = new OpenFileDialog();
            OPEN.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            //이미지를 선택하면
            if (OPEN.ShowDialog() == DialogResult.OK)
            {
                IMAGE = new Bitmap(OPEN.FileName);  //비트맵 열기
                IMAGE = bc.bmp_find_rotate(IMAGE);  //수평선에 맞게 회전하고
                list = bc.bmp_dot(IMAGE);           //교차점 알아내고

                Bitmap bm = new Bitmap(IMAGE);      //이미지를 복사하고(원본이미지를 보호)
                bm = bc.bmp_binary(bm);             //이진화

                int[] blank;        //공백 좌표 xl = 0, yt = 0, xr = 0, yb = 0;

                int x_lt, y_lt;     //왼쪽위 좌표
                int x_rb, y_rb;     //오른쪽아래 좌표

                //url 저장할 리스트
                List<String> url_list = new List<string>();

                //url생성
                String url_tmp = webBrowser1.Url.AbsoluteUri;   //url가져와서
                url_tmp = url_tmp.Split('?')[1];                //?기준으로 자른 오른쪽꺼를
                String url_tmp_post = "http://로타리종합물류.com/home/write1_post_test.php?" + url_tmp;    //호스트주소 넣고
                String url_data = "";  //데이터넣을 url


                Boolean endflag = false;    //종료 플래그



                for (int j = 1; j < 24; j++)   //지정된 개수만큼 반복
                {
                    y_lt = list[1][j] + 5;      //y_lt축 점으로부터 공백 5
                    y_rb = list[1][j + 1] - 5;  //y_rb축 점으로부터 공백 5
                    for (int i = 0; i < 9; i++)
                    {
                        x_lt = list[0][i] + 5;          //x_lt축 점으로부터 공백 5
                        x_rb = list[0][i + 1] - 5;      //x_rb축 점으로부터 공백 5

                        blank = bc.bmp_blank(IMAGE, x_lt, y_lt, x_rb, y_rb);    //공백찾고

                        Bitmap cut;
                        int cut_start, cut_length;
                        int[] blank2;

                        //범위를 못찾으면 종료
                        if (blank[0] >= blank[2] || blank[1] >= blank[3])
                        {
                            endflag = true;
                            break;
                        }

                        //0번째 칸이면(월)
                        if (i == 0)
                        {
                            cut = bc.bmp_cut(bm, blank[0], blank[1], blank[2], blank[3]); //찾은 좌표대로 자르고
                            cut = bc.bmp_resize(cut, 20, 20);                       //20x20으로 리사이즈

                            //gen(cut); //특징데이터 생성(histo 리스트 갱신)
                            gen_f(cut, "0", "tmp.txt", FileMode.Create);
                            url_data += "&date2="+Program.SVM_Classification(Program.model_0).ToString(); //인식결과


                            bc.bmp_binary(cut);                                     //이진화
                            cut.Save(".\\img\\img_" + j.ToString() + "_" + i.ToString() + ".png");

                        }
                        //1번째 칸이면(일)
                        if (i == 1)
                        {
                            cut = bc.bmp_cut(bm, blank[0], blank[1], blank[2], blank[3]); //찾은 좌표대로 자르고
                            cut = bc.bmp_resize(cut, 20, 20);                       //20x20으로 리사이즈

                            //gen(cut); //특징데이터 생성(histo 리스트 갱신)
                            gen_f(cut, "0", "tmp.txt", FileMode.Create);
                            url_data += "&date3=" + Program.SVM_Classification(Program.model_0).ToString(); //인식결과

                            bc.bmp_binary(cut);                                     //이진화
                            cut.Save(".\\img\\img_" + j.ToString() + "_" + i.ToString() + ".png");

                        }
                        //2번째 칸이면(요일)
                        if (i == 2)
                        {
                            cut = bc.bmp_cut(bm, blank[0], blank[1], blank[2], blank[3]); //찾은 좌표대로 자르고
                            cut = bc.bmp_resize(cut, 20, 20);                       //20x20으로 리사이즈

                            //gen(cut); //특징데이터 생성(histo 리스트 갱신)
                            gen_f(cut, "0", "tmp.txt", FileMode.Create);
                            url_data += "&yo=" + Program.SVM_Classification(Program.model_2).ToString(); //인식결과

                            bc.bmp_binary(cut);                                     //이진화
                            cut.Save(".\\img\\img_" + j.ToString() + "_" + i.ToString() + ".png");

                        }
                        //3번째 칸이면(도착지)
                        if (i == 3)
                        {
                            cut = bc.bmp_cut(bm, blank[0], blank[1], blank[2], blank[3]); //찾은 좌표대로 자르고
                            cut = bc.bmp_resize(cut, 60, 30);                       //40x20으로 리사이즈

                            //gen(cut); //특징데이터 생성(histo 리스트 갱신)
                            gen_f(cut, "0", "tmp.txt", FileMode.Create);
                            url_data += "&ddo=" + Program.SVM_Classification(Program.model_3).ToString(); //인식결과

                            bc.bmp_binary(cut);                                     //이진화
                            cut.Save(".\\img\\img_" + j.ToString() + "_" + i.ToString() + ".png");

                        }

                        //4번째 칸이면(차량번호)
                        if (i == 4)
                        {
                            
                            cut = bc.bmp_cut(bm, blank[0], blank[1], blank[2], blank[3]); //찾은 좌표대로 자르고
                            cut = bc.bmp_resize(cut, 60, 30);                       //40x20으로 리사이즈

                            //gen(cut); //특징데이터 생성(histo 리스트 갱신)
                            gen_f(cut, "0", "tmp.txt", FileMode.Create);
                            url_data += "&number=" + Program.SVM_Classification(Program.model_4).ToString(); //인식결과

                            bc.bmp_binary(cut);                                     //이진화
                            cut.Save(".\\img\\img_" + j.ToString() + "_" + i.ToString() + ".png");
                            
                        }

                        //6번째 칸이면(금액)
                        if (i == 6)
                        {
                            /*
                            cut = bc.bmp_cut(bm, blank[0], blank[1], blank[2], blank[3]); //찾은 좌표대로 자르고
                            cut = bc.bmp_resize(cut, 20, 20);                       //60x20으로 리사이즈

                            //gen(cut); //특징데이터 생성(histo 리스트 갱신)
                            gen_f(cut, "0", "tmp.txt", FileMode.Create);
                            url_data += "&money=" + Program.SVM_Classification(Program.model_6).ToString(); //인식결과

                            bc.bmp_binary(cut);                                     //이진화
                            cut.Save(".\\img\\img_" + j.ToString() + "_" + i.ToString() + ".png");
                            */

                            x_lt = list[0][i] + 5;          //x_lt축 점으로부터 공백 5
                            x_rb = list[0][i + 1] - 5;      //x_rb축 점으로부터 공백 5

                            int money_add = 0;  //금액 누적
                            int money_pos = 100;    //금액위치
                            for (int i_t = 0; i_t < 3; i_t++)   //3칸 반복
                            {
                                //첫번째숫자
                                cut_length = (list[0][i+1] - list[0][i]) / 3;   //자를 길이
                                cut_start = list[0][i] + cut_length * i_t;  //자르기 시작

                                //자른부분 다시 공백찾고
                                blank2 = bc.bmp_blank(IMAGE, cut_start + 5, blank[1], cut_start + cut_length - 5, blank[3]);

                                //범위를 못찾으면 종료 지나가기
                                if (blank2[0] >= blank2[2] || blank2[1] >= blank2[3])
                                {
                                    money_pos = money_pos / 10;
                                    continue;
                                }

                                cut = bc.bmp_cut(bm, blank2[0], blank2[1], blank2[2], blank2[3]); //찾은 좌표대로 자르고
                                cut = bc.bmp_resize(cut, 20, 20);                       //20x20으로 리사이즈

                                //gen(cut); //특징데이터 생성(histo 리스트 갱신)
                                gen_f(cut, "0", "tmp.txt", FileMode.Create);
                                String tmp = Program.SVM_Classification(Program.model_0).ToString(); //인식결과

                                money_add = money_add + (money_pos * int.Parse(tmp));   //자리수만큼 더하고
                                money_pos = money_pos / 10;

                                bc.bmp_binary(cut);                                     //이진화
                                cut.Save(".\\img\\img_" + j.ToString() + "_" + i.ToString() + "_" + i_t.ToString() + ".png");

                                //색깔그리기
                                for (int x = blank2[0] - 1; x <= blank2[2] + 1; x++)
                                {
                                    IMAGE.SetPixel(x, blank2[1] - 1, Color.Red);
                                    IMAGE.SetPixel(x, blank2[3] + 1, Color.Red);
                                }
                                for (int y = blank2[1] - 1; y <= blank2[3] + 1; y++)
                                {
                                    IMAGE.SetPixel(blank2[0] - 1, y, Color.Red);
                                    IMAGE.SetPixel(blank2[2] + 1, y, Color.Red);
                                }
                            }

                            url_data += "&money=" + money_add.ToString(); //인식결과
                        }

                        //7번째 칸이면(메모)
                        if (i == 8)
                        {
                            cut = bc.bmp_cut(bm, blank[0], blank[1], blank[2], blank[3]); //찾은 좌표대로 자르고
                            cut = bc.bmp_resize(cut, 40, 20);                       //40x20으로 리사이즈

                            //gen(cut); //특징데이터 생성(histo 리스트 갱신)
                            gen_f(cut, "0", "tmp.txt", FileMode.Create);
                            url_data += "&memo1=" + Program.SVM_Classification(Program.model_7).ToString(); //인식결과

                            bc.bmp_binary(cut);                                     //이진화
                            cut.Save(".\\img\\img_" + j.ToString() + "_" + i.ToString() + ".png");

                        }

                        //금액이 아닐때만 그리기
                        if (i != 6)
                        {

                            //색깔그리기
                            for (int x = blank[0] - 1; x <= blank[2] + 1; x++)
                            {
                                IMAGE.SetPixel(x, blank[1] - 1, Color.Red);
                                IMAGE.SetPixel(x, blank[3] + 1, Color.Red);
                            }
                            for (int y = blank[1] - 1; y <= blank[3] + 1; y++)
                            {
                                IMAGE.SetPixel(blank[0] - 1, y, Color.Red);
                                IMAGE.SetPixel(blank[2] + 1, y, Color.Red);
                            }
                        }

                    }

                    if (endflag) break; //종료플래그가 뜨면 끝내버리기


                    //줄바꿈(임시)
                    //label2.Text += url_data+"\n";

                    //입력하기
                    String urls = url_tmp_post + url_data;           //뒷주소를 넣는다
                    url_list.Add(urls); //링크를 리스트에 쌓아두고

                    url_data = "";   //데이터 초기화
                }

                //마지막에 일괄처리
                for(int i=0;i<url_list.Count;i++)
                {
                    webBrowser1.Navigate(new Uri(url_list[i])); //하나씩 입력
                    //브라우저 로딩이 완료되었을때까지 대기
                    while(webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                    }
                }
            }

            IMAGE.Save(".\\test.png");
        }
        //콤보박스
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: now_select = 0; break;
                case 1: now_select = 1; break;
                case 2: now_select = 2; break;
                case 3: now_select = 3; break;
                case 4: now_select = 4; break;
                case 5: now_select = 5; break;
                case 6: now_select = 6; break;
                case 7: now_select = 7; break;
            }
        }

        //학습버튼
        private void button2_Click(object sender, EventArgs e)
        {
            switch (now_select)
            {
                case 0:
                case 1: Program.model_0 = Program.SVM_GenModel("wine0.txt"); break; //월, 일
                case 2: Program.model_2 = Program.SVM_GenModel("wine2.txt"); break; //요일
                case 3: Program.model_3 = Program.SVM_GenModel("wine3.txt"); break; //도착지
                case 4: Program.model_4 = Program.SVM_GenModel("wine4.txt"); break; //차량번호
                case 6: Program.model_6 = Program.SVM_GenModel("wine6.txt"); break; //금액
                case 7: Program.model_7 = Program.SVM_GenModel("wine7.txt"); break; //메모
                default:
                    break;
            }
        }
        //모델로드
        private void button6_Click(object sender, EventArgs e)
        {
            Program.model_0 = Program.SVM_LoadModel("model_wine0.txt"); //월
            //Program.model_1 = Program.SVM_LoadModel("model_wine1.txt"); //
            Program.model_2 = Program.SVM_LoadModel("model_wine2.txt"); //
            Program.model_3 = Program.SVM_LoadModel("model_wine3.txt"); //
            Program.model_4 = Program.SVM_LoadModel("model_wine4.txt"); //
            //Program.model_5 = Program.SVM_LoadModel("model_wine5.txt"); //
            Program.model_6 = Program.SVM_LoadModel("model_wine6.txt"); //
            Program.model_7 = Program.SVM_LoadModel("model_wine7.txt"); //
        }
        
    }
}
