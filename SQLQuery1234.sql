select *  from m_ERPMQC_REALTIME 
order by CAST(inspectdate as datetime) + CAST(inspecttime as datetime) desc

select * from m_MQCQR_Record