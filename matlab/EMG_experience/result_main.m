clear
vaf_list = [];
loss_list = [];
for gap = 1:1
    for numSyn = 3:5
        name = sprintf("./result/%d/result_%d.mat", gap, numSyn);
        result = load(name);
        vaf_list(gap+1-1, numSyn-2) = mean(result.vafs);
        loss_list(gap+1-1, numSyn-2) = result.loss;
    end
end