select * from m_ERPMQC_REALTIME_Test where inspectdate > '20191230' and line ='L03' 
order by inspecttime DESC

select sum(convert(int,data)) as tongOP from m_ERPMQC_REALTIME_Test where line ='L04' and remark ='OP'


delete from m_ERPMQC_REALTIME_Test 