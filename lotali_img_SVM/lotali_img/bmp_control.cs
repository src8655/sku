using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lotali_img
{
    public class bmp_control
    {
        public static int POINT_LENGTH = 4; //왼쪽위좌표찾기 임계값
        Color color;
        Point point;

        public bmp_control()
        {
            point = new Point(0, 0);
        }
        //이진화
        public Bitmap bmp_binary(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                {
                    color = bmp.GetPixel(i, j);
                    if (color.R <= 200)
                        bmp.SetPixel(i, j, Color.Black);
                    else bmp.SetPixel(i, j, Color.White);
                }
            return bmp;
        }
        //라인 회전
        public Bitmap bmp_find_rotate(Bitmap bm)
        {
            bm = bmp_resize(bm, 1000, 1400);
            Bitmap bmp = bmp_binary(new Bitmap(bm));

            List<float> angle_list = new List<float>();
            Boolean breakpoint;
            int cnt;
            //왼쪽위 좌표 찾기
            for (int i = 0; i < bmp.Height / 4; i++)
            {
                breakpoint = false;
                cnt = 0;
                for (int j = 0; j < bmp.Width / 4; j++)
                {
                    color = bmp.GetPixel(j, i);
                    //검정색이면 카운터증가
                    if (color.R <= 200) cnt++;
                    if (cnt >= POINT_LENGTH)    //임계값을 넘으면 Y좌표기억
                    {
                        point.Y = i;
                        breakpoint = true;
                        break;
                    }
                }
                if (breakpoint) break;
            }
            for (int i = 0; i < bmp.Width / 4; i++)
            {
                breakpoint = false;
                cnt = 0;
                for (int j = 0; j < bmp.Height / 4; j++)
                {
                    color = bmp.GetPixel(i, j);
                    //검정색이면 카운터증가
                    if (color.R <= 200) cnt++;
                    if (cnt >= POINT_LENGTH)    //임계값을 넘으면 Y좌표기억
                    {
                        point.X = i;
                        breakpoint = true;
                        break;
                    }
                }
                if (breakpoint) break;
            }
            //회전하면서 라인찾기
            Bitmap bmp_tmp;
            float k = -2.0f;
            while (k <= 2.0f)
            {
                cnt = 0;
                bmp_tmp = bmp_rotate(bmp, k);
                for (int i = 0; i < bmp.Width; i++)
                {
                    color = bmp_tmp.GetPixel(i, point.Y);
                    //검정색이면 카운터증가
                    if (color.R <= 200) cnt++;
                    if (cnt >= bmp_tmp.Width/2)  //라인찾으면 리스트에추가
                    {
                        angle_list.Add(k);
                        break;
                    }
                }
                k = k + 0.1f;
            }
            float angle = 0.0f;
            if(angle_list.Count != 0)
                angle = angle_list[angle_list.Count / 2];

            bm = bmp_rotate(bm, angle); //원본을 회전하고
            //bm = bmp_binary(bm);        //다시 이진화

            return bm;
        }
        //비트맵 회전
        public Bitmap bmp_rotate(Bitmap bmp, float angle)
        {
            Bitmap bmps = new Bitmap(bmp);
            Graphics GFX = Graphics.FromImage(bmps);
            GFX.TranslateTransform(point.X, point.Y);
            GFX.RotateTransform(angle); //회전
            GFX.TranslateTransform(-point.X, -point.Y);
            GFX.DrawImage(bmps, new Point(0,0));

            return bmps;
        }
        //비트맵 회전2
        public Bitmap bmp_rotate2(Bitmap bmp, float angle)
        {
            Bitmap bmps = new Bitmap(bmp);
            Graphics GFX = Graphics.FromImage(bmps);
            GFX.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
            GFX.RotateTransform(angle); //회전
            GFX.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
            GFX.DrawImage(bmps, new Point(0, 0));

            return bmps;
        }
        //교차점 찾기
        public List<int>[] bmp_dot(Bitmap bmp)
        {
            //리턴할 교차점
            List<int>[] list = new List<int>[2];
            list[0] = new List<int>();
            list[1] = new List<int>();

            Bitmap bmps = bmp_binary(new Bitmap(bmp));  //원본 복사

            bmps = bmp_binary(bmps);    //이진화

            int cnt;
            //가로라인 찾기
            for (int j = 0; j < bmps.Height; j++)
            {
                cnt = 0;
                for (int i = 0; i < bmps.Width; i++)
                {
                    color = bmps.GetPixel(i, j);
                    if (color.R <= 200) cnt++;
                }
                //찾음
                if (cnt > (bmps.Width / 5)*3)
                {
                    list[1].Add(j);   //y좌표 추가
                    /*bmp.SetPixel(0, j, Color.Red);
                    bmp.SetPixel(1, j, Color.Red);
                    bmp.SetPixel(2, j, Color.Red);
                    bmp.SetPixel(3, j, Color.Red);*/
                    j = j + 20;
                }
            }
            //세로라인 찾기
            for (int i = 0; i < bmps.Width; i++)
            {
                cnt = 0;
                for (int j = 0; j < bmps.Height; j++)
                {
                    color = bmps.GetPixel(i, j);
                    if (color.R <= 200) cnt++;
                }
                //찾음
                if (cnt > (bmps.Height / 7)*4)
                {
                    list[0].Add(i);   //x좌표 추가
                    /*bmp.SetPixel(i, 0, Color.Red);
                    bmp.SetPixel(i, 1, Color.Red);
                    bmp.SetPixel(i, 2, Color.Red);
                    bmp.SetPixel(i, 3, Color.Red);*/
                    i = i + 40;
                }
            }
            
            //표의 교차점을 빨간색으로 칠하기
            for (int i = 0; i < list[0].Count; i++)
            {
                for (int j = 0; j < list[1].Count; j++)
                {
                    bmp.SetPixel(list[0][i], list[1][j], Color.Red);
                }
            }
            return list;
        }
        //이미지 크기
        public Bitmap bmp_resize(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.DrawImage(b, 0, 0, nWidth, nHeight);
                return result;
            }
        }
        //이미지 자르기
        public Bitmap bmp_cut(Bitmap b, int x1, int y1, int x2, int y2)
        {
            //가로 세로길이 구하고
            int nWidth = x2 - x1 + 1;
            int nHeight = y2 - y1 + 1;
            Bitmap result = new Bitmap(nWidth, nHeight);    //크기만큼 비트맵 생성

            for (int i = x1; i <= x2; i++)
                for (int j = y1; j <= y2; j++)
                    result.SetPixel(i-x1, j-y1, b.GetPixel(i, j));  //새 비트맵에 저장

            return result;
        }
        //공백 제거된 좌표 구하기 (이진화된 이미지를 줘야함)
        public int[] bmp_blank(Bitmap bm, int x_lt, int y_lt, int x_rb, int y_rb)
        {
            int[] result = new int[4];//int xl = 0, xr = 0, yt = 0, yb = 0;

            //왼쪽공백
            Boolean endflag = false;        //반복문종료 플래그
            for (int ii = x_lt; ii < x_rb; ii++)
            {
                for (int jj = y_lt; jj < y_rb; jj++) //왼쪽부터 세로로 스캔하다가
                {
                    if (bm.GetPixel(ii, jj).R < 200) //검정색을만나면
                    {
                        result[0] = ii;            //왼쪽위 x를 갱신
                        endflag = true;     //종료플래그를 올리고
                        break;              //현재루프를 종료
                    }
                }
                if (endflag) break;
            }
            //위쪽공백
            endflag = false;        //반복문종료 플래그
            for (int jj = y_lt; jj < y_rb; jj++)
            {
                for (int ii = x_lt; ii < x_rb; ii++) //위부터 가로로 스캔하다가
                {
                    if (bm.GetPixel(ii, jj).R < 200) //검정색을만나면
                    {
                        result[1] = jj;            //왼쪽위 x를 갱신
                        endflag = true;     //종료플래그를 올리고
                        break;              //현재루프를 종료
                    }
                }
                if (endflag) break;
            }
            //오른쪽공백
            endflag = false;        //반복문종료 플래그
            for (int ii = x_rb; ii > x_lt; ii--)
            {
                for (int jj = y_lt; jj < y_rb; jj++) //오른쪽부터 세로로 스캔하다가
                {
                    if (bm.GetPixel(ii, jj).R < 200) //검정색을만나면
                    {
                        result[2] = ii;            //오른쪽아래 x를 갱신
                        endflag = true;     //종료플래그를 올리고
                        break;              //현재루프를 종료
                    }
                }
                if (endflag) break;
            }
            //아래쪽공백
            endflag = false;        //반복문종료 플래그
            for (int jj = y_rb; jj > y_lt; jj--)
            {
                for (int ii = x_lt; ii < x_rb; ii++) //아래부터 가로로 스캔하다가
                {
                    if (bm.GetPixel(ii, jj).R < 200) //검정색을만나면
                    {
                        result[3] = jj;            //왼쪽위 x를 갱신
                        endflag = true;     //종료플래그를 올리고
                        break;              //현재루프를 종료
                    }
                }
                if (endflag) break;
            }

            return result;
        }

    }
}
