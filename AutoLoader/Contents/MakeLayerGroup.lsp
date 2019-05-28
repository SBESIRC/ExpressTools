(defun TH:MakeLayerGroupFilter ( parent group layers )
    (command "._-layer" "filter" "new" "group" parent "0" group "")
    (foreach layer layers
        (command "._-layer" "filter" "edit" group "add" layer "")
    )
    (command "._-layer" "filter" "edit" group "delete" "0" "")
); End of defun

(defun TH:DeleteLayerGroupFilter ( group )
    (command "._-layer" "filter" "delete" group "")
); End of defun

(defun TH:RenameLayerGroupFilter ( oldName newName )
    (command "._-layer" "filter" "rename" oldName newName "")
); End of defun

(defun TH:AddToLayerGroup ( layer group)
    (command "._-layer" "filter" "edit" group "add" layer "")
); End of defun