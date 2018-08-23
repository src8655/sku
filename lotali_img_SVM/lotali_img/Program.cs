using LibSVMsharp.Helpers;
using LibSVMsharp.Extensions;
using LibSVMsharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lotali_img
{
    static class Program
    {
        public static SVMModel model_0;     //월
        public static SVMModel model_1;     //일
        public static SVMModel model_2;     //요일
        public static SVMModel model_3;     //도착지
        public static SVMModel model_4;     //차량번호
        public static SVMModel model_5;     //이름
        public static SVMModel model_6;     //금액
        public static SVMModel model_7;     //메모
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }
        //인식 하기
        public static int SVM_Classification(SVMModel md)
        {
            int result = 0;

            SVMProblem testSet = SVMProblemHelper.Load("tmp.txt");    //인식데이터셋 열기
            testSet = testSet.Normalize(SVMNormType.L2);
            double[] testResults = testSet.Predict(md);

            result = (int)testResults[0];

            return result;
        }
        //학습모델 생성
        public static SVMModel SVM_GenModel(String dataset)
        {
            SVMProblem trainingSet = SVMProblemHelper.Load(dataset); //학습데이터셋 열기

            trainingSet = trainingSet.Normalize(SVMNormType.L2);
            
            SVMParameter parameter = new SVMParameter();
            parameter.Type = SVMType.C_SVC;
            parameter.Kernel = SVMKernelType.RBF;
            parameter.C = 1;
            parameter.Gamma = 1;
            
            double[] crossValidationResults;
            int nFold = 5;
            trainingSet.CrossValidation(parameter, nFold, out crossValidationResults);
            double crossValidationAccuracy = trainingSet.EvaluateClassificationProblem(crossValidationResults);

            SVMModel model = trainingSet.Train(parameter);  // 학습된 모델 생성
            SVM.SaveModel(model, "model_" + dataset);       // 모델 저장
            return model;
        }
        //학습모델 로드
        public static SVMModel SVM_LoadModel(String model)
        {
            return SVM.LoadModel(model);
        }
    }
}
