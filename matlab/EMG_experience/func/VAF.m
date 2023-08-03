function v = VAF(y, y_est)

    % Variance Accounted For (VAF) | Percentage value (%)
    %
    % v = vaf(y, y_est)
    %
    % y     : measured output (real)
    % y_est : estimated output
    %
    
    v = (1- norm(y - y_est)^2/norm(y)^2);
    
    if ( v < 0 )
        v = 0;
    end

end
