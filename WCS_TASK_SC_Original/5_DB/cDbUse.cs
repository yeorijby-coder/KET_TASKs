using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

namespace WCS_TASK_SC
{  
    class cDbUse : cDbBaseOra   //@.상속
    {
         //@. 일반적인 PC Client에서 Global Connection 객체 1개 사용 시
        public cDbUse()
        {
            //(GM_CN_DB, pBind);
            this.DbBaseOra(ref cDefApp.GM_DB1_CN, false);
        }
        public  cDbUse(bool pBind)
        {
            //(GM_CN_DB, pBind);
            this.DbBaseOra(ref cDefApp.GM_DB1_CN, pBind);
        }

    //@. cBaseDb마다 Connection을 별도로 가져가야 할 경우, PDA Server
    //@. Backgroud Process에서 cBaseDb의 Connection객체를 사용 할 경우
    //@. strDummy는 Overload 함수 정의 시 구분하기 위한 파라미터, 즉 의미가 없다.
    //@. ex) Public BDb As New cDbUse("Multi", False)
    public  cDbUse(string pStrDummy, bool pBind )
    {
        this.DbBaseOra(pBind); //@.Backgroud Process에서 CBaseDb의 Connection객체를 사용 할 경우(//@. 종료시 comMain.Close를 반드시 호출)
    }

    //@. Connection이 2개 이상일 경우 (지정), 지정된 갯수만 필요할 때
    public  cDbUse(ref OleDbConnection  pCon, bool pBind)
    {
        this.DbBaseOra(ref pCon, pBind);
    }

    ////@. 메세지 보이기 재정의 하여 사용 할 것
    //    public static new void ShowErrMsg(bool pMsgBox)
    //    {
    //        //@. 메세지 바꾸기
    //        switch (DB_LOCK)
    //        {
    //            case DB_LOCK :ErrMsg = "잠시 후 다시 사용하세요. (DB Lock.)";
    //            case DB_DUP:ErrMsg = "이미 등록된 자료입니다. (DB 중복)";
    //        }

    //        //@.//@. 메세지박스 보이기를 선택 한 경우
    //        //@.If bMsgBox Then
    //        //@.    ShowMsgClient(ErrMsg, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    //        //@.End If
    //    }
    }
}
