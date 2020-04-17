select * from m_ERPMQC_REALTIME where inspectdate >= '20200317' 
order by inspectdate

select *  from m_ERPMQC_REALTIME where  inspectdate >=  '20200317' and lot not like '%0010;B01;B01%'

select * from m_ERPMQC_REALTIME where cast (inspectdate as date)>= '20200317' and model ='BMH32222268-31370286'

select sum(cast (data as int)) as tong, item from m_ERPMQC_REALTIME where  cast (inspectdate as date)>= '20200317' and model ='BMH32222268-31370286'
group by item



select line,sum(cast (data as int)) as tong, remark from m_ERPMQC_REALTIME where 
cast (inspectdate as date) =  '20200318' and inspecttime >= '11:47:00' and line = 'L01'
group by remark, line
select line,sum(cast (data as int)) as tong, remark from m_ERPMQC_REALTIME where 
cast (inspectdate as date) =  '20200318' and inspecttime >= '11:47:00' and line = 'L02'
group by remark, line
select line,sum(cast (data as int)) as tong, remark from m_ERPMQC_REALTIME where 
cast (inspectdate as date) =  '20200318' and inspecttime >= '11:47:00' and line = 'L03'
group by remark, line

select line,sum(cast (data as int)) as tong, remark from m_ERPMQC_REALTIME where 
cast (inspectdate as date) =  '20200318' and inspecttime >= '09:31:00' and line = 'L02'
group by remark, line


delete from m_ERPMQC_REALTIME where cast (inspectdate as date) =  '20200318' and inspecttime > '08:00:00' and line ='L01'




select * from m_ERPMQC_REALTIME where cast (inspectdate as date) =  '20200318' and inspecttime >= '09:31:00' and line = 'L02'
order by inspecttime



select * from m_ERPMQC_REALTIME 
where cast (inspectdate as datetime) + cast (inspecttime as datetime) >= '20200320 11:00:00'
and line = 'L04'
order by inspecttime


select * from m_ERPMQC_REALTIME 
where cast (inspectdate as datetime) + cast (inspecttime as datetime) >  '20200320 08:30' 
and line = 'L03'
order by cast (inspectdate as datetime) + cast (inspecttime as datetime)

--update  m_ERPMQC_REALTIME set data = data+3 where serno ='B511-20020081;0010;B01;B01-20200319_160713'

select * from m_ERPMQC_REALTIME
where cast (inspectdate as date) =  '20200319' and inspecttime >= '12:30' and line ='L02'
group by line, remark
order by line

select line, model,SUM(CAST(data as int)) as Tong , remark ,min(inspecttime) as Mintime, MAX(inspecttime) as MaxTime
from m_ERPMQC_REALTIME where inspectdate = '20200319' and inspecttime >='12:30' --and inspecttime <='13:45:00'
--and line ='L01'
group by model, line, remark
order by model

select isnull(sum(cast( isnull(data,'0') as int)),0) from m_ERPMQC_REALTIME 
where lot = 'B511-18120023;0010;B01;B01' and remark != 'RW'

select line, model,SUM(CAST(data as int)) as Tong ,min(cast (inspectdate as datetime) + cast (inspecttime as datetime)) as Mintime, MAX(cast (inspectdate as datetime) + cast (inspecttime as datetime)) as MaxTime
from m_ERPMQC_REALTIME 
where 
cast (inspectdate as datetime) + cast (inspecttime as datetime) >= '20200325 11:00:00'
and  cast (inspectdate as datetime) + cast (inspecttime as datetime) <= '20200326 11:00:00'
--and model like '%32222268%' 
group by model, line
order by model

select line, lot, model,SUM(CAST(data as int)) as Tong , remark ,min(cast (inspectdate as datetime) + cast (inspecttime as datetime)) as Mintime, MAX(cast (inspectdate as datetime) + cast (inspecttime as datetime)) as MaxTime
from m_ERPMQC_REALTIME  where 1=1 --
and model like '%056%'
and cast (inspectdate as datetime) + cast (inspecttime as datetime) >= '20200411 11:00:00'
and  cast (inspectdate as datetime) + cast (inspecttime as datetime) <= '20200413 11:00:00'
group by model, line, remark,lot
order by model

---3/23/2020 9:16:16 PM-3/23/2020 9:23:24 PM
select * from  m_ERPMQC_REALTIME where 1=1 
and model like '%751%'
and cast (inspectdate as datetime) + cast (inspecttime as datetime) >= '20200411 11:00:00'
and  cast (inspectdate as datetime) + cast (inspecttime as datetime) <= '20200411 17:00:00'
--and model like '%068%' 
and remark ='Reset'
order by cast (inspectdate as datetime) + cast (inspecttime as datetime)


select * from m_ERPMQC_REALTIME where 1=1
--and model like '%056%'
--and line ='L01'
and cast (inspectdate as datetime) + cast (inspecttime as datetime) >= '20200417 08:00:00'
and  cast (inspectdate as datetime) + cast (inspecttime as datetime) <= '20200417 11:00:00'
order by cast (inspectdate as datetime) + cast (inspecttime as datetime)


