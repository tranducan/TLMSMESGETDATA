
select * from (
select model, remark, CAST(data as int) as SL from m_ERPMQC_REALTIME 
where 1=1
and CAST(inspectdate as datetime) +  CAST(inspecttime as datetime) >= '20200619 11:00:00'
and  CAST(inspectdate as datetime) +  CAST(inspecttime as datetime) <= '20200622 11:00:00'
) t pivot (sum(SL) for remark in (
[OP],[NG],[RW] )) as Sum_Of_Quantity



select * from (
select model, remark, CAST(data as int) as SL from m_ERPMQC_REALTIME 
where 1=1
and CAST(inspectdate as datetime) +  CAST(inspecttime as datetime) >= '20200618 11:00:00'
and  CAST(inspectdate as datetime) +  CAST(inspecttime as datetime) <= '20200619 11:00:00'
) t pivot (sum(SL) for remark in (
[OP],[NG],[RW] )) as Sum_Of_Quantity

select * from (
select model, remark, CAST(data as int) as SL from m_ERPMQC_REALTIME 
where 1=1
and CAST(inspectdate as datetime) +  CAST(inspecttime as datetime) >= '20200625 11:00:00'
and  CAST(inspectdate as datetime) +  CAST(inspecttime as datetime) <= '2020062 11:00:00'
) t pivot (sum(SL) for remark in (
[OP],[NG],[RW] )) as Sum_Of_Quantity


select * from m_ERPMQC_REALTIME where 
CAST(inspectdate as datetime) +  CAST(inspecttime as datetime) >= '20200625 11:00:00'