select * from m_ERPMQC_REALTIME where serno like '%B511-20040003;0010;B01;B01-20200713%'


select * from m_ERPMQC_REALTIME where lot = 'B511-20040003;0010;B01;B01'

select * from m_ERPMQC_REALTIME 
where 1=1 
and (CAST(inspectdate as datetime) + CAST(inspecttime as datetime)) >= '20200610' 
and line = 'L04'  and model like '%200%'
order by CAST(inspectdate as datetime) + CAST(inspecttime as datetime)

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040010;0010;B01;B01-20200713_121559','B511-20040010;0010;B01;B01','BMH1249200S02',
'B01', 'TECHLINK','L04','MQC','OUTPUT','2020-07-13','12:15:59','41','0', '','OP')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040010;0010;B01;B01-20200713_121559','B511-20040010;0010;B01;B01','BMH1249200S02',
'B01', 'TECHLINK','L04','MQC','RW3','2020-07-13','12:15:59','3','0', '','RW')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040002;0010;B01;B01-20200713_120859','B511-20040002;0010;B01;B01','BWTXMANC00161-01A',
'B01', 'TECHLINK','L03','MQC','RW6','2020-07-13','12:12:59','1','0', '','RW')
insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040002;0010;B01;B01-20200713_120859','B511-20040002;0010;B01;B01','BWTXMANC00161-01A',
'B01', 'TECHLINK','L03','MQC','RW9','2020-07-13','12:12:59','1','0', '','RW')
insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040002;0010;B01;B01-20200713_120859','B511-20040002;0010;B01;B01','BWTXMANC00161-01A',
'B01', 'TECHLINK','L03','MQC','RW17','2020-07-13','12:12:59','5','0', '','RW')


insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040003;0010;B01;B01-20200713_121001','B511-20040003;0010;B01;B01','BMH1257096S04',
'B01', 'TECHLINK','L02','MQC','RW6','2020-07-13','12:10:59','11','0', '','RW')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040003;0010;B01;B01-20200713_121001','B511-20040003;0010;B01;B01','BMH1257096S04',
'B01', 'TECHLINK','L02','MQC','RW14','2020-07-13','12:10:59','6','0', '','RW')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040008;0010;B01;B01-20200618_170859','B511-20040008;0010;B01;B01','BMH1257070S03',
'B01', 'TECHLINK','L04','MQC','OUTPUT','2020-06-18','17:08:59','95','0', '','OP')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040018;0010;B01;B01-20200604_110959','B511-20040018;0010;B01;B01','BMH1227768S11',
'B01', 'TECHLINK','L04','MQC','RW9','2020-06-04','11:09:30','6','0', '','RW')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040018;0010;B01;B01-20200604_111059','B511-20040018;0010;B01;B01','BMH1227768S11',
'B01', 'TECHLINK','L04','MQC','RW17','2020-06-04','11:10:30','4','0', '','RW')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040018;0010;B01;B01-20200604_111159','B511-20040018;0010;B01;B01','BMH1227768S11',
'B01', 'TECHLINK','L04','MQC','RW14','2020-06-04','11:11:30','1','0', '','RW')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20040006;0010;B01;B01-20200520_084030','B511-20040006;0010;B01;B01','BMH1284056S02',
'B01', 'TECHLINK','L02','MQC','RW17','2020-05-20','08:40:30','1','0', '','RW')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20020011;0010;B01;B01-20200519_171954','B511-20020011;0010;B01;B01','BMH32222268-31370286',
'B01', 'TECHLINK','L02','MQC','RW15','2020-05-19','17:19:54','10','0', '','RW')

insert into m_ERPMQC_REALTIME (serno, lot,model,site,factory,line,process,item,inspectdate,inspecttime,data,judge,status,remark)
values ('B511-20020011;0010;B01;B01-20200519_170654','B511-20020011;0010;B01;B01','BMH32222268-31370286',
'B01', 'TECHLINK','L02','MQC','RW17','2020-05-19','17:06:54','3','0', '','RW')

select * from m_process
