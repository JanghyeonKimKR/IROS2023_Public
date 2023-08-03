% This function is called by IOCsolver_IROS2023

function ln = PropCostate(l,ss,cvec)

global ts

m=2; n=8; o=8;    

dphi_ds = 2*eye(n) .* ss';
df_ds = Get_df_ds_mat(ss);

ln = ts*dphi_ds*cvec + [-eye(n) + ts*df_ds]*l;

end