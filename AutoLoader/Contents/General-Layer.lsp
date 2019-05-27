;; Unique  -  Lee Mac
;; Returns a list with duplicate elements removed.
(defun LM:Unique ( l / x r )
    (while l
        (setq x (car l)
              l (vl-remove x (cdr l))
              r (cons x r)
        )
    )
    (reverse r)
)

;; Unique-p  -  Lee Mac
;; Returns T if the supplied list contains distinct items.
(defun LM:Unique-p ( l )
    (vl-every (function (lambda ( x ) (not (member x (setq l (cdr l)))))) l)
)

;; List Duplicates  -  Lee Mac
;; Returns a list of items appearing more than once in a supplied list
(defun LM:ListDupes ( l / x r )
    (while l
        (if (member (setq x (car l)) (cdr l))
            (setq r (cons x r))
        )
        (setq l (vl-remove x (cdr l)))
    )
    (reverse r)
)

;; Count Items  -  Lee Mac
;; Returns a list of dotted pairs detailing the number of
;; occurrences of each item in a supplied list.
(defun LM:CountItems ( l / c l r x )
    (while l
        (setq x (car l)
              c (length l)
              l (vl-remove x (cdr l))
              r (cons (cons x (- c (length l))) r)
        )
    )
    (reverse r)
)

(defun TH:MakeLayerGroupFilter ( parent group layers )
    (command "._-layer" "filter" "new" "group" parent "0" group "")
    (foreach layer layers
        (command "._-layer" "filter" "edit" group "add" layer "")
    )
    (command "._-layer" "filter" "edit" group "delete" "0" "")
); End of defun

(defun TH:RenameLayerGroupFilter ( oldName newName )
    (command "._-layer" "filter" "rename" oldName newName "")
); End of defun

(defun TH:AddToLayerGroup ( layer group)
    (command "._-layer" "filter" "edit" group "add" layer "")
); End of defun

(defun TH:loadCSV ( file / data )
    (if (setq data (LM:readcsv file))
        (progn
        (regapp "AcAecLayerStandard")
            (foreach line data
		(setq name		(nth 2 line))
                (setq color		(atoi (nth 6 line)))
                (setq ltype		(nth 7 line))
                (setq plot	 	(= (nth 11 line) "T"))
                (setq desc		(nth 12 line))
                (MakeLayer name color ltype -3 plot 0 desc)
            )
        ; layer group filters
        (setq groups nil)
        (foreach line data
                  (setq groups (cons (nth 0 line) groups))    		
        )
        ; remove duplicated group
        (LM:Unique groups)
        ; create parent layer group filter
        (setq root "全部")
        (setq parent "天华标准图层")
        (TH:MakeLayerGroupFilter root parent nil)
        ; create sub layer group filter
        (foreach group groups
                  (TH:MakeLayerGroupFilter parent group nil)
        )
        ; add layers to the layer group filters
        (foreach line data
                  (TH:AddToLayerGroup (nth 2 line) (nth 0 line))
        )
        )
    )
    (princ)
)

(defun c:THALC ()
    (LM:loadlinetypes '("HIDDEN" "CENTER") nil)
    (setq pluginPath (strcat (getenv "PROGRAMFILES") "\\Autodesk\\ApplicationPlugins\\ThCADPlugin.bundle"))
    (setq pluginContentPath (strcat pluginPath "\\Contents"))
    (TH:loadCSV (strcat pluginContentPath "\\Standards\\Layer\\Architecture.csv"))
    (TH:RenameLayerGroupFilter "天华标准图层" "天华建筑标准图层")
    (princ)
);defun

(defun c:THSLC ()
    (LM:loadlinetypes '("CENTER" "DASHED" "DASHED2") nil)
    (setq pluginPath (strcat (getenv "PROGRAMFILES") "\\Autodesk\\ApplicationPlugins\\ThCADPlugin.bundle"))
    (setq pluginContentPath (strcat pluginPath "\\Contents"))
    (TH:loadCSV (strcat pluginContentPath "\\Standards\\Layer\\Structure.csv"))
    (TH:RenameLayerGroupFilter "天华标准图层" "天华结构标准图层")
    (princ)
);defun

(defun c:THMLC ()
    (LM:loadlinetypes '("HIDDEN" "CENTER2" "DASHED2") nil)
    (setq pluginPath (strcat (getenv "PROGRAMFILES") "\\Autodesk\\ApplicationPlugins\\ThCADPlugin.bundle"))
    (setq pluginContentPath (strcat pluginPath "\\Contents"))
    (TH:loadCSV (strcat pluginContentPath "\\Standards\\Layer\\HVAC.csv"))
    (TH:RenameLayerGroupFilter "天华标准图层" "天华暖通标准图层")
    (princ)
);defun

(defun c:THELC ()
    (LM:loadlinetypes '("HIDDEN" "HIDDEN2" "DASHDOT" "DASHDOT2" "DIVIDE2" "E-GND" "E-THU" "PHANTOM" "PHANTOM2" "BORDER2" "CENTER" "DIVIDE" "DIVIDE2" "DASHED") nil)
    (setq pluginPath (strcat (getenv "PROGRAMFILES") "\\Autodesk\\ApplicationPlugins\\ThCADPlugin.bundle"))
    (setq pluginContentPath (strcat pluginPath "\\Contents"))
    (TH:loadCSV (strcat pluginContentPath "\\Standards\\Layer\\Electrical.csv"))
    (TH:RenameLayerGroupFilter "天华标准图层" "天华电气标准图层")
    (princ)
);defun

(defun c:THPLC ()
    (LM:loadlinetypes '("CENTER2" "BORDER" "DASHED" "DASHDOT") nil)
    (setq pluginPath (strcat (getenv "PROGRAMFILES") "\\Autodesk\\ApplicationPlugins\\ThCADPlugin.bundle"))
    (setq pluginContentPath (strcat pluginPath "\\Contents"))
    (TH:loadCSV (strcat pluginContentPath "\\Standards\\Layer\\Plumbing.csv"))
    (TH:RenameLayerGroupFilter "天华标准图层" "天华给排水标准图层")
    (princ)
);defun