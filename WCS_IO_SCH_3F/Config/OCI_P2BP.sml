<S1F1 P On line Request
  Undefined Structure
>

<S1F3 P Selected Equipment Status Request
  <L n  
    <A n SVID : Status Variable ID >
  >
>

<S1F2 S On Line Data from Host
  Undefined Structure
>

<S1F2 S On Line Data from Equipment
  <L n  
    <L 2  
      <A 40 MDLN : Equipment Model Name >
      <A 40 SOFTREV : Softeware Revision >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 101 : Current Equipment Status >
      <L 3  
        <A 40 EQPNAME : Equipment Name >
        <A 40 CRST : Control State 0:OffLine 1:OnLine Local 2:OnLine Remote >
        <A 40 STKST : Stocker State0:Down 1:Run >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 102 : Current Port Status >
      <L n n:Port Count 
        <L 3  
          <A 40 PTID : Port ID >
          <A 40 PTST : Port State 0:Down, 1:Run >
          <A 40 DIRECTION : Port Type 1:In Port, 2:Out Port >
        >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 201 : Current Stokcer Transfer Command >
      <L 5  
        <A 64 COMMANDID : Unique Transfer Command ID >
        <A 40 LOTID : Lot ID >
        <A 40 BOXID : Box ID >
        <A 40 SOURLOC : From Location >
        <A 40 DESTLOC : To Location >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 202 : Current Breaking Zone Transfer Command >
      <L n n:Command Count 
        <L 3  
          <A 64 COMMANDID : Unique Transfer Command ID >
          <A 40 LOTID : Lot ID >
          <A 40 BOXID : Box ID >
        >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 203 : Current Bagging Zone Transfer Command >
      <L n n:Command Count 
        <L 4  
          <A 64 COMMANDID : Unique Transfer Command ID >
          <A 40 LOTID : Lot ID >
          <A 40 BOXID : Box ID >
          <A 40 GRADE : Product Grade >
        >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 204 : All Transfer Command Information In Bag Packing Controller >
      <L n n:Command Count 
        <L 4  
          <A 64 COMMANDID : Unique Transfer Command ID >
          <A 40 SECTION : Transfer Section 1:Stocker, 2:Breaking, 3:Bagging >
          <A 40 LOTID : Lot ID >
          <A 40 BOXID : Box ID >
        >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 205 : All Lot Information In Bag Packing Controller >
      <L n n:Lot Count 
        <L 2  
          <A n LOTID : Lot ID >
          <A 40 SECTION : Transfer Section 1:Stocker, 2:Breaking, 3:Bagging >
        >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 206 : Inventory Zone Box Information >
      <L n n : Box Count 
        <L 4  
          <A 40 BOXID : Box ID >
          <A 40 LOTID : Lot ID >
          <A 40 BOXST : Box State 1:Transfer, 2:Stored, 3:Alt Stored, 6:Blocked >
          <A 40 LOCATION : Box Store Location >
        >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 207 : Inventory Zone Capacity >
      <L n n : Zone Count 
        <L 2  
          <A 40 ZONENAME : Zone Name >
          <A 40 ZONECAPA : Zone Capacity >
        >
      >
    >
  >
>

<S1F4 S Selected Equipment Status Data
  <L n  
    <L 2  
      <A 40 SVID 208 : Store Information >
      <L 3  
        <A 40 TOTALLOCNS : Total Number of Shelf in a Stocker.  Shelf Capacity >
        <A 40 ACTUALLOCNS : Total Number of Occupied Shelf in a Stocker >
        <A 40 ACTUALBOXS : The Number of Actual Box.  Exclusive Empty Box >
      >
    >
  >
>

<S1F5 P Formatted Status Request
  <L 2  
    <A 40 STKID : Stocker ID >
    <A 40 SFCD : Status Format Code.  0:EQP Status, 1:Inventory Data Report >
  >
>

<S1F6 S Formatted Status Data
  <L 3  
    <A 40 SFCD : Status Format Code  0:EQP Status, 1~4:Inventory Data Report >
    <L 3  
      <A 40 STKID : Stocker ID >
      <A 40 CRST : Control State 0:Off Line, 1:On Line Local, 2:On Line Remote >
      <A 40 STKST : Stocker State 0:Down, 1:Run >
    >
    <L n  
      <A n ITEM >
    >
  >
>

<S1F13 P Establish Communication Request
  <L 2  
    <A n MDLN : Equipment Model Name >
    <A n SOFTREV : Software Revision >
  >
>

<S1F14 S Establish Communication Acknowledge
  <L 2  
    <A 40 COMMACK : Establish Communication Acknowledge code  0: Acknowledge, 1: Error >
    <L 2  
      <A n MDLN : Equipment Model Name >
      <A n SOFTREV : Software Revision >
    >
  >
>

<S1F15 P Request OFF LINE
  Undefined Structure
>

<S1F16 S OFF LINE Acknowledge
  <A 40 OFLACK : Off Line Acknowledge 0: Accepted, other: Error >
>

<S1F17 P Request ON LINE
  Undefined Structure
>

<S1F18 S ON LINE Acknowledge
  <A 40 ONLACK : On Line Acknowledge 0: Accepted, other: Error >
>

<S2F17 P Date and Time Data Request
  Undefined Structure
>

<S2F18 S Date and Time Data Acknowledge
  <A n TIME : Date and Time  format: YYYYMMDDHHMMSS >
>

<S2F31 P Date and Time Set Request
  <A n TIME : Date and Time  format: YYYYMMDDHHMMSS >
>

<S2F32 S Date and Time Data Acknowledge
  <A 40 TIACK : Date and Time Set Acknowledge 0: Acknowledge, 1: Error >
>

<S2F41 P Host Command Send  INSTALL
  <L 2  
    <A 40 RCMD : 'INSTALL' >
    <L 7  
      <L 2  
        <A 40 CPNAME : 'BOXID' >
        <A 40 CPVAL : Box ID >
      >
      <L 2  
        <A 40 CPNAME : 'LOTID' >
        <A 40 CPVAL : Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'BOXWEIGHT' >
        <A 40 CPVAL : Box Weight >
      >
      <L 2  
        <A 40 CPNAME : 'EMPTYBOXWEIGHT' >
        <A 40 CPVAL : Empty Box Weight >
      >
      <L 2  
        <A 40 CPNAME : 'CHUCKREMOVE' >
        <A 40 CPVAL : Chuck Remove >
      >
      <L 2  
        <A 40 CPNAME : 'EMPTYBOX' >
        <A 40 CPVAL : Empty Box Flag >
      >
      <L 2  
        <A 40 CPNAME : 'BOXLOC' >
        <A 40 CPVAL : Box Location >
      >
    >
  >
>

<S2F41 P Host Command Send  LOCATE
  <L 2  
    <A 40 RCMD : 'LOCATE' >
    <L 1  
      <L 2  
        <A 40 CPNAME : 'BOXID' >
        <A 40 CPVAL : Box ID >
      >
    >
  >
>

<S2F41 P Host Command Send  REMOVE
  <L 2  
    <A 40 RCMD : 'REMOVE' >
    <L 1  
      <L 2  
        <A 40 CPNAME : 'BOXID' >
        <A 40 CPVAL : Box ID >
      >
    >
  >
>

<S2F41 P Host Command Send  REMOVE
  <L 2  
    <A 40 RCMD : 'REMOVE' >
    <L 1  
      <L 2  
        <A 40 CPNAME : 'BOXID' >
        <A 40 CPVAL : Box ID >
      >
    >
  >
>

<S2F41 P Host Command Send  UPDATE
  <L 2  
    <A 40 RCMD : 'UPDATE' >
    <L 6  
      <L 2  
        <A 40 CPNAME : 'BOXID' >
        <A 40 CPVAL : Box ID >
      >
      <L 2  
        <A 40 CPNAME : 'LOTID' >
        <A 40 CPVAL : Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'BOXWEIGHT' >
        <A 40 CPVAL : Box Weight >
      >
      <L 2  
        <A 40 CPNAME : 'EMPTYBOXWEIGHT' >
        <A 40 CPVAL : Empty Box Weight >
      >
      <L 2  
        <A 40 CPNAME : 'CHUCKREMOVE' >
        <A 40 CPVAL : Chuck Remove >
      >
      <L 2  
        <A 40 CPNAME : 'EMPTYBOX' >
        <A 40 CPVAL : Empty Box Flag >
      >
    >
  >
>

<S2F41 P Host Command Send  CANCEL
  <L 2  
    <A 40 RCMD : 'CANCEL' >
    <L 1  
      <L 2  
        <A 40 CPNAME : 'COMMANDID' >
        <A 40 CPVAL : Command ID >
      >
    >
  >
>

<S2F41 P Host Command Send  ABORT
  <L 2  
    <A 40 RCMD : 'ABORT' >
    <L 1  
      <L 2  
        <A 40 CPNAME : 'COMMANDID' >
        <A 40 CPVAL : Command ID >
      >
    >
  >
>

<S2F41 P Host Command Send  PAUSE
  <L 2  
    <A 40 RCMD : 'PAUSE' >
    <L 0  
    >
  >
>

<S2F41 P Host Command Send  RESUME
  <L 2  
    <A 40 RCMD : 'RESUME' >
    <L 0  
    >
  >
>

<S2F41 P Host Command Send  Label Information
  <L 2  
    <A 40 RCMD : 'LABELINFO' >
    <L 4  
      <L 2  
        <A 40 CPNAME : 'LABELTYPE' >
        <A 40 CPVAL : Label Type  CCE, PE >
      >
      <L 2  
        <A 40 CPNAME : 'LOTID' >
        <A 64 CPVAL : Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'BOXID' >
        <A 64 CPVAL : Box ID >
      >
      <L 2  
        <A 40 CPNAME : 'LABELINFO' >
        <A 100 CPVAL : Auto Label Information >
      >
    >
  >
>

<S2F41 P Host Command Send  Sample Picking
  <L 2  
    <A 40 RCMD : 'SAMPLE' >
    <L 4  
      <L 2  
        <A 40 CPNAME : 'FROMLOTID' >
        <A 40 CPVAL : From Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'FROMLOTWEIGHT' >
        <A 40 CPVAL : From Lot Weight >
      >
      <L 2  
        <A 40 CPNAME : 'TOLOTID' >
        <A 40 CPVAL : To Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'TOLOTWEIGHT' >
        <A 40 CPVAL : To Lot Weight >
      >
    >
  >
>

<S2F41 P Host Command Send  C.C.E Remove List Request
  <L 2  
    <A 40 RCMD : 'REMOVEREQ' >
    <L 1  
      <L 2  
        <A 40 CPNAME : 'REACTORID' >
        <A 40 CPVAL >
      >
    >
  >
>

<S2F41 P Host Command Send  Prime Etching & Pre Etching
  <L 2  
    <A 40 RCMD : 'PRIMEETCHING' Or 'PREETCHING' >
    <L 2  
      <L 2  
        <A 40 CPNAME : 'FROMLOTID' >
        <A 40 CPVAL : From Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'FROMLOTWEIGHT' >
        <A 40 CPVAL : From Lot Weight >
      >
    >
  >
>

<S2F41 P Host Command Send  PE Bag Merge Process
  <L 2  
    <A 40 RCMD : 'PEMERGE' >
    <L 5  
      <L 2  
        <A 40 CPNAME : 'FROMLOTID' >
        <A 40 CPVAL : From Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'FROMLOTWEIGHT' >
        <A 40 CPVAL : From Lot Weight >
      >
      <L 2  
        <A 40 CPNAME : 'TOLOTID' >
        <A 40 CPVAL : To Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'TOLOTWEIGHT' >
        <A 40 CPVAL : To Lot Weight >
      >
      <L 2  
        <A 40 CPNAME : 'MERGELOTIDS' >
        <A 100 CPVAL : Merge Lot ID >
      >
    >
  >
>

<S2F41 P Host Command Send  Batch Weight Revision
  <L 2  
    <A 40 RCMD : 'WEIGHTREVISION' >
    <L 2  
      <L 2  
        <A 40 CPNAME : 'LOTID' >
        <A 40 CPVAL : Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'LOTWEIGHT' >
        <A 40 CPVAL : Lot Weight >
      >
    >
  >
>

<S2F41 P Host Command Send  Loss Process
  <L 2  
    <A 40 RCMD : 'LOSSPROCESS' >
    <L 2  
      <L 2  
        <A 40 CPNAME : 'LOTID' >
        <A 40 CPVAL : Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'LOSSWEIGHT' >
        <A 40 CPVAL : Loss Weight >
      >
    >
  >
>

<S2F41 P Host Command Send  Lot Information
  <L 2  
    <A 40 RCMD : 'LOTINFO' >
    <L 3  
      <L 2  
        <A 40 CPNAME : 'TOLOTID' >
        <A 40 CPVAL : Text >
      >
      <L 2  
        <A 40 CPNAME : 'TOPRODUCTID' >
        <A 40 CPVAL : Text >
      >
      <L 2  
        <A 40 CPNAME : 'BOXNO' >
        <A 40 CPVAL : Text >
      >
    >
  >
>

<S2F41 P Host Command Send  Lot Information Modify
  <L 2  
    <A 40 RCMD : 'LOTMODIFY' >
    <L 3  
      <L 2  
        <A 40 CPNAME : 'FROMLOT' >
        <A 40 CPVAL : Text >
      >
      <L 2  
        <A 40 CPNAME : 'TOLOT' >
        <A 40 CPVAL : Text >
      >
      <L 2  
        <A 40 CPNAME : 'BOXNO' >
        <A 40 CPVAL : Text >
      >
    >
  >
>

<S2F41 P Host Command Send  Product Change
  <L 2  
    <A 40 RCMD : 'POCHANGE' >
    <L 2  
      <L 2  
        <A 40 CPNAME : 'LOTID' >
        <A 40 CPVAL : Lot ID >
      >
      <L 2  
        <A 40 CPNAME : 'TOPRODUCTID' >
        <A 40 CPVAL : To Product ID >
      >
    >
  >
>

<S2F41 P Host Command Send  Operation Call
  <L 2  
    <A 40 RCMD : 'OPERATIONCALL' >
    <L 1  
      <L 2  
        <A 40 CPNAME : 'TEXT' >
        <A 40 CPVAL : Text >
      >
    >
  >
>

<S2F42 S Host Command Acknowledge HCACK != 3
  <L 2  
    <A 40 HCACK : Host Acknowledge  0: Acknowledge, 1: Error >
    <L 0  
    >
  >
>

<S2F42 S Host Command Acknowledge HCACK = 3
  <L 2  
    <A 40 HCACK : Host Acknowledge  0: Acknowledge, 1: Error >
    <L n n = Number of Parameter Groups 
      <L 2  
        <A 40 CPNAME : Command Parameter Name >
        <A 40 CPACK : Control Parameter Ack Code >
      >
    >
  >
>

<S2F49 P Enhanced Remote Command
  <L 2  
    <A 40 RCMD : 'TRANSFER' >
    <L 2  
      <L 2  
        <A 40 CPNAME : 'COMMANDINFO' >
        <L 2  
          <L 2  
            <A 40 CPNAME : COMMANDID >
            <A 64 CPVAL : Command ID >
          >
          <L 2  
            <A 40 CPNAME : PRIORITY >
            <A 40 CPVAL : Priority >
          >
        >
      >
      <L 2  
        <A 40 CPNAME : 'TRANSFERINFO' >
        <L 3  
          <L 2  
            <A 40 CPNAME : LOTID >
            <A 40 CPVAL : Lot ID >
          >
          <L 2  
            <A 40 CPNAME : SOURCE >
            <A 40 CPVAL : Source  Shelf, ZoneName, PortID >
          >
          <L 2  
            <A 40 CPNAME : DEST >
            <A 40 CPVAL : Dest  Shelf, ZoneName, PortID >
          >
        >
      >
    >
  >
>

<S2F50 S Enhanced Remote Command Acknowledge HCACK != 3
  <L 2  
    <A 40 HCACK : Host Acknowledge  0: Acknowledge, 1: Error >
    <L 4  
      <A 64 COMMANDID >
      <A 40 LOTID >
      <A 40 SOURCE >
      <A 40 DEST >
    >
  >
>

<S2F50 S Enhanced Remote Command Acknowledge HCACK = 3
  <L 2  
    <A 40 HCACK : Host Acknowledge  0: Acknowledge, 1: Error >
    <L n n = Number of Parameter Groups 
      <L 2  
        <A 40 CPNAME : Command Parameter Name >
        <A 40 CPACK : Control Parameter Ack Code >
      >
    >
  >
>

<S5F1 P Alarm Report Send
  <L 4  
    <A n ALCD : Alarm Code  1: On, 0:Off >
    <A n ALLV : Alarm Level  0: Heavy, 1: Middle, 2: Light >
    <A n ALID : Alarm ID >
    <A n ALTX : Alarm Text >
  >
>

<S5F2 S Alarm Report Acknowledge
  <A n ACKC5 : Alarm Acknowledge CODE >
>

<S6F9 P Event Report Send
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  461:Breaking Zone Arrive, 462:Breaking Box Change, 463:Breaking Process Completed >
    <L 2  
      <A 40 RPTID 461 : Breaking Zone Transfer Report >
      <L 6  
        <A 64 COMMANDID : Unique Transfer Command ID >
        <A 40 LOTID : Lot ID >
        <A 40 CVDBOXID : CVD Box ID >
        <A 40 BREBOXID : Breaking Box ID >
        <A 40 PRODGRADE : Product Grade >
        <A 40 PRODWEIGHT : Product Weight >
      >
    >
  >
>

<S6F9 P Event Report Send
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <A 40 RPTID 501 : Bagging Zone Transfer Report >
      <L 12  
        <A 64 COMMANDID : Unique Transfer Command ID >
        <A 40 FROMLOTID : From Lot ID >
        <A 40 TOLOTWEIGHT : To Lot Weight >
        <A 40 TOPRODUCTID : To Product ID >
        <A 40 MODULEID : Module ID >
        <A 40 CVDBOXID : CVD Box ID >
        <A 40 BREBOXID : Breaking Box ID >
        <A 40 INBOXID : Inner Box ID >
        <A 40 OUTBOXID : Outer Box ID >
        <A 40 PRODGRADE : Product Grade >
        <A 40 MANUALBAG : Manual Bagging Y:Manual Bagging, N:Auto Process >
        <A 100 LABELINFO : Auto Label Information >
      >
    >
  >
>

<S6F10 S Event report acknowledge
  <A n ACK : Event report acknowledge,  0:normal >
>

<S6F11 P Event Report Send  Stocker Status Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <A 40 RPTID 101 : Stocker Status Report >
      <L 3  
        <A 40 STKID : Equipment ID >
        <A 40 CRST : Control State >
        <A 40 STKST : Stocker State >
      >
    >
  >
>

<S6F11 P Event Report Send  Transfer Status Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <A 40 RPTID 102 : Transfer Status Report >
      <L 13  
        <A 40 BOXID : Box ID >
        <A 40 LOTID : Lot ID >
        <A 40 JOBTYPE : Job Type  01:Store, 02:Retrieve, 03:RackToRack, 04:StationToStation >
        <A 40 SHELFID : Shelf ID  Location >
        <A 40 BOXWEIGHT : Box Weight >
        <A 40 EBOXWEIGHT : Empty Box Weight >
        <A 40 PRODUCTID : Product ID >
        <A 40 CCEREMOVEFLAG : Carbon Chuck Remove  Y:Remove, N:Not Remove >
        <A 40 PORTID : Port ID >
        <A 40 RESULTCODE : Result Code  00: No Problem, 01:Source Empty >
        <A 40 SOURSHELF : From Shelf Name  Location >
        <A 40 DESTSHELF : To Shelf Name  Location >
        <A 40 EMPTYFLAG : Box Type 0: Empty Box, 1:Product Box >
      >
    >
  >
>

<S6F11 P Event Report Send  Box Status Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  155:Box Install Completed, 156:Box Remove Completed >
    <L 2  
      <A 40 RPTID 103 : Box Status Report >
      <L 7  
        <A 40 LOTID : Lot ID >
        <A 40 BOXID : Box ID >
        <A 40 BOXWEIGHT : Box Weight >
        <A 40 EBOXWEIGHT : Empty Box Weight >
        <A 40 CHUCKREMOVE : Carbon Chuck Remove  Y:Remove, N:Not Remove >
        <A 40 SHELFID : Shelf ID  Location >
        <A 40 EMPTYFLAG : Box Type 0: Empty Box, 1:Product Box >
      >
    >
  >
>

<S6F11 P Event Report Send  Port Status Change Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  211:Port State Change Event, 212:Weigher State Change Event >
    <L 2  
      <A 40 RPTID 105 : Port Status Change Report >
      <L 3  
        <A 40 PTID : Port ID >
        <A 40 DIRECTION : Port Type  1:In Port, 2:Out Port >
        <A 40 PTST : Port State  0:Down, 1:Run >
      >
    >
  >
>

<S6F11 P Event Report Send  Box Information Event Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <A 40 RPTID 106 : Box Information Event Report >
      <L 8  
        <A 64 ZONENAME : Zone Name >
        <A 40 DIRECTION : Port Type  1:In Port, 2:Out Port >
        <A 40 BATCHLOTID : Batch Lot ID >
        <A 40 PRODUCTID : Product ID >
        <A 40 PORTID : Port ID >
        <A 40 BOXID : Box ID >
        <A 40 BOXWEIGHT : Weight Check Value >
        <A 40 EMPTYFLAG : Box Type  0:Empty Box, 1:Product Box >
      >
    >
  >
>

<S6F11 P Event Report Send  Box Information Event Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <A 40 RPTID 106 : Box Information Event Report >
      <L 8  
        <A 64 ZONENAME : Zone Name >
        <A 40 DIRECTION : Port Type  1:In Port, 2:Out Port >
        <A 40 BATCHLOTID : Batch Lot ID >
        <A 40 PRODUCTID : Product ID >
        <A 40 PORTID : Port ID >
        <A 40 BOXID : Box ID >
        <A 40 BOXWEIGHT : Weight Check Value >
        <A 40 EMPTYFLAG : Box Type  0:Empty Box, 1:Product Box >
      >
    >
  >
>

<S6F11 P Event Report Send  Box Information Change Event Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  181:Product Grade Change, 182:Box Weight Change >
    <L 2  
      <A 40 RPTID 107 : Box Information Change Event Report >
      <L 3  
        <A 40 BOXID : Box ID >
        <A 40 PRODGRADE : Product Grade >
        <A 40 BOXWEIGHT : Weight Check Value >
      >
    >
  >
>

<S6F11 P Event Report Send  Zone Capacity Change
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  161:Zone Capacity Change >
    <L 2  
      <A 40 RPTID 108 : Zone Capacity Change >
      <L 4  
        <A 40 ZONENAME : Zone Name >
        <A 40 TOTALLOCNS : Total Number of Shelf in a Stocker.  Shelf Capacity >
        <A 40 ACTUALLOCNS : Total Number of Occupied Shelf In Stocker >
        <A 40 ACTUALCSTS : The Number of Actual Box >
      >
    >
  >
>

<S6F11 P Event Report Send  Box Status Change
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  151:Box Transferring, 152:Box Stored, 153:Box Resume >
    <L 2  
      <A 40 RPTID 109 : Box Status Change >
      <L 4  
        <A 40 BOXID : Box ID >
        <A 40 ZONENAME : Zone Name >
        <A 40 LOCATION : Location Zone Name >
        <A 40 BOXST : Box State 0:Normal, 1:Transfer, 2:Stored, 3:Alt Stored,6:Blocked Prohibit >
      >
    >
  >
>

<S6F11 P Event Report Send  Location Status Change
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  702:Location State Change >
    <L 2  
      <A 40 RPTID 110 : Location Status Change >
      <L 2  
        <A 40 LOCATION : Location >
        <A 40 LOCSTATE : Location State  0:Normal, 1:Block >
      >
    >
  >
>

<S6F11 P Event Report Send  Zone Full Status
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  162:Zone Full >
    <L 2  
      <A 40 RPTID 111 : Zone Full Status >
      <L 2  
        <A 40 ZONENAME : Zone Name >
        <A 40 ZONESTATE : Zone State  0:Normal, 1:Zone Full >
      >
    >
  >
>

<S6F11 P Event Report Send  Location Block Status
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  703:Location State Change >
    <L 2  
      <A 40 RPTID 112 : Location Block Status >
      <L 4  
        <A 40 BOXID : Box ID >
        <A 40 ZONENAME : Zone Name >
        <A 40 LOCATION : Location >
        <A 40 LOCSTATE : Location State  0:Normal, 1:Block >
      >
    >
  >
>

<S6F11 P Event Report Send  Duplicate Box ID Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  704:Duplicate Box ID Report, 705:Double Storage >
    <L 2  
      <A 40 RPTID 114 : Duplicate Box ID Report >
      <L 4  
        <A 40 ZONENAME : Zone Name >
        <A 40 LOCATION : Shelf ID  Location >
        <A 40 LOTID : Lot ID >
        <A 40 BOXID : Box ID >
      >
    >
  >
>

<S6F11 P Event Report Send  Host Command Result Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  171:Lot Locate, 172:Box Locate, 173:Box Update >
    <L 2  
      <A 40 RPTID 115 : Host Command Result Report >
      <L 6  
        <A 40 LOTID : Lot ID >
        <A 40 BOXID : Box ID >
        <A 40 BOXWEIGHT : Box Weight >
        <A 40 EBOXWEIGHT : Empty Box Weight >
        <A 40 CHUCKREMOVE : Carbon Chuck Remove  Y:Remove, N:Not Remove >
        <A 40 LOCATION : Shelf ID  Location >
      >
    >
  >
>

<S6F11 P Event Report Send  Stocker Active/Idle
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  201:Stocker Active, 202:Stocker Idle >
    <L 2  
      <A 40 RPTID 151 : Stocker Active/Idle >
      <L 2  
        <A 40 EQPID : Equipment ID >
        <A 40 EQPSTATE : Equipment State  0:Idle, 1:Active >
      >
    >
  >
>

<S6F11 P Event Report Send  Carbon Chuck Remove Zone Transfer Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <A 40 RPTID 401 : Carbon Chuck Remove Zone Transfer Report >
      <L 8  
        <A 40 FROMLOTID : From Lot ID >
        <A 40 TOLOTWEIGHT : To Lot Weight  NEW >
        <A 100 CCEIDS : C.C.E ID >
        <A 40 CCEWEIGHTS : C.C.E Weight >
        <A 40 BOXID : Box ID >
        <A 40 BOXINDEX : Box Index FRT:First, MID:Middle, LST:Last >
        <A 40 CHUCKREMOVE : Carbon Chuck Remove  Y:Remove, N:Not Remove >
        <A 40 FROMLOTWEIGHT : Old Box Weight >
      >
    >
  >
>

<S6F11 P Event Report Send  Sample Picking in C.C.E Remove Port
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  421:Sample Picking >
    <L 2  
      <A 40 RPTID 421 : Sample Picking in C.C.E Remove Port >
      <L 6  
        <A 40 BOXID : Box ID >
        <A 40 LOTID : Lot ID >
        <A 40 PICKINGPOINT : Sample Picking Point  1:C.C.E Remove Port, 2:Manual Bagging Zone >
        <A 40 PICKINGTYPE : Picking Type >
        <A 40 BATCHWEIGHT : Batch Total Weight >
        <A 40 PICKINGWEIGHT : Picking Weight >
      >
    >
  >
>

<S6F11 P Event Report Send  Sample Picking
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  422:Coring Sample Picking, 424:Filament Breaking Move, 425:Pre Etching Move, 427:Fall Down >
    <L 2  
      <A 40 RPTID 422 : Sample Picking in C.C.E Remove Port >
      <L 2  
        <A 40 FROMLOTID : From Lot ID >
        <A 40 TOLOTWEIGHT : To Lot Weight >
      >
    >
  >
>

<S6F11 P Event Report Send  Sample Picking
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  423:Metal Sample Picking, 426:Filament Move >
    <L 2  
      <A 40 RPTID 423 : Sample Picking >
      <L 2  
        <A 40 LOTID : Lot ID >
        <A 40 LOSSWEIGHT : Loss Weight >
      >
    >
  >
>

<S6F11 P Event Report Send  Bagging Zone Transfer Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <A 40 RPTID 501 : Bagging Zone Transfer Report >
      <L 17  
        <A 40 FROMLOTID : From Lot ID  OLD >
        <A 40 TOLOTWEIGHT : To Lot Weight >
        <A 40 TOPRODUCTID : To Product ID  P.O >
        <A 40 MODULEID : Module ID  Bagging Location  1~4 >
        <A 40 MERGELOTIDS : Merge Lot ID >
        <A 40 BOXID : Inner Box ID >
        <A 40 OUTBOXID : Outer Box ID >
        <A 40 CURRZONE : Current Process Zone >
        <A 40 PRODGRADE : Product Grade >
        <A 40 PRODWEIGHT : Product Weight >
        <A 40 ETRAYWEIGHT : Empty Tray Weight >
        <A 40 BATCHWEIGHT : Batch Total Weight >
        <A 40 TRANSWEIGHT : Bagging Transferring Weight >
        <A 40 COMPWEIGHT : Bagging Complete Weight >
        <A 40 REMAILWEIGHT : Remain Weight >
        <A 40 MANUALBAG : Manual Bagging Y:Manual Bagging, N:Auto Process >
        <A 100 LABELINFO : Auto Label Information >
      >
    >
  >
>

<S6F11 P Event Report Send  PE Bag Remain Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  521:PE Bag Remain Process >
    <L 2  
      <A 40 RPTID 520 : PE Bag Remain Report >
      <L 5  
        <A 40 LOTID : Lot ID >
        <A 40 MODULEID : Module ID  Bagging Location  1~4 >
        <A 40 LOCATION : Cell Location >
        <A 40 REMAINWEIGHT : Remain Weight >
        <A 40 PRODGRADE : Product Grade >
      >
    >
  >
>

<S6F11 P Event Report Send  U Rod Box Move
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <A 40 RPTID 550 >
      <L 12  
        <A 40 BOXID : Box ID >
        <A 40 LOTID : Lot ID >
        <A 40 SHELFID : Shelf ID >
        <A 40 BOXWEIGHT : Box Weight >
        <A 40 EMPTYWEIGHT : Empty Box Weight >
        <A 40 PRODUCTID : Product ID >
        <A 40 CCEREMOVEFLAG : Carbon Chuck Remove  Y:Remove, N:Not Remove >
        <A 40 CCID : Carbon Chuck ID >
        <A 40 CCIDCNT : Carbon Chuck Count >
        <A 40 BOXINDEX : Box Index FRT:First, MID:Middle, LST:Last >
        <A 40 BOXSEQID : Box Sequence ID >
        <A 40 TOAREACODE : To Area Code  P1, P2 >
      >
    >
  >
>

<S6F11 P Event Report Send  Product Information Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  711:Product Information Report >
    <L 2  
      <A 40 RPTID 711 : Product Information Report >
      <L 2  
        <A 40 LOTID : From Lot ID >
        <A 40 TOPRODUCTID : To Product ID >
      >
    >
  >
>

<S6F11 P Event Report Send  Batch Weight Revision Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  721:Batch Weight Revision >
    <L 2  
      <A 40 RPTID 721 : Batch Weight Revision Report >
      <L 2  
        <A 40 LOTID : Lot ID >
        <A 40 TOLOTWEIGHT : To Lot Weight >
      >
    >
  >
>

<S6F11 P Event Report Send  Transfer Undo Report
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID  801:Transferring Undo >
    <L 2  
      <A 40 RPTID 801 : Transfer Undo Report >
      <L 8  
        <A 40 LOTID : Lot ID >
        <A 40 CVDBOXID : CVD Box ID >
        <A 40 BREBOXID : Breaking Box ID >
        <A 40 INBOXID : Inner Box ID >
        <A 40 OUTBOXID : Outer Box ID >
        <A 40 CURRZONE : Current Process Zone >
        <A 40 PRODGRADE : Product Grade >
        <A 40 PRODWEIGHT : Product Weight >
      >
    >
  >
>

<S6F11 P Event Report Send  U-Rod Box Weight Check
  <L 3  
    <A 40 DATAID : Data ID >
    <A 40 CEID : Collection Event ID >
    <L 2  
      <L 2  
        <A 40 RPTID 500 : AGV INFO >
        <L 9  
          <A 40 AGVID : AGV ID >
          <A 40 REACTORID : Reactor ID >
          <A 40 LOTID : Lot ID >
          <A 40 LOTSUBID : Lot Sub ID >
          <A 40 BOXID : Box ID >
          <A 40 SEQID : Lot Sequence ID >
          <A 40 SEQNO : Lot Sequence Number >
          <A 40 USERID : User ID >
          <A 40 DATE : Date >
        >
      >
      <L 2  
        <A 40 RPTID 501 : Box INFO >
        <L 6  
          <A 40 TOTWEIGHT : Total Weight >
          <A 40 BOXWEIGHT : BOX Weight >
          <A 40 ORGWEIGHT : (Total-Box) Weight >
          <A 40 BOXINFO : Box Info >
          <A 40 CCID : Carbon Chuck ID >
          <A 40 CCCNT : Carbon Chuck Count >
        >
      >
    >
  >
>

<S6F12 S Event report acknowledge
  <A n ACK : Event report acknowledge  0: normal >
>

<S6F23 P Request Spooled Data
  Undefined Structure
>

<S6F24 S Request Spooled Data Acknowledge
  <A n RSDA : Request Spooled Data Acknowledge >
>

<S9F13 P Transaction Timer Time Out
  <L 2  
    <A n MEXP : Message Expected SxFyyy >
    <A n EDID : Expected Data Identification >
  >
>

<S10F1 P Terminal Display Request
  <L 2  
    <A n TID : Terminal ID >
    <A n TEXT : Text >
  >
>

<S10F2 S Terminal request acknowledge
  <A 40 ACKC10 : Terminal request Acknowledge CODE >
>

<S10F3 P Single Block Terminal Display Request
  <L 2  
    <A n TID : Terminal ID >
    <A n TEXT : Text >
  >
>

<S10F4 S Single Block Terminal Display acknowledge
  <A 40 ACKC10 : Multi Block Terminal Display acknowledge CODE >
>

<S99F1 P Lot Info Request
  <L 2  
    <A 40 LOTID : Lot ID >
    <A 40 BOXID : Box ID >
  >
>

<S99F2 S Lot Info
  <L 2  
    <A 40 LOTID : Lot ID >
    <L 4  
      <A 40 LOTID : Lot ID >
      <A 40 BOXID : Box ID >
      <A 40 BOXWEIGHT : Box & U Rod Weight >
      <A 40 EBOXWEIGHT : Empty Box Weight >
    >
  >
>

<S99F2 S Lot Info
  <A 40 LIACK : Lot Info Acknowledge CODE  0: Normal, other: Error >
>

