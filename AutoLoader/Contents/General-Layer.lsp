(defun TH:loadCSV ( file / data )
    (if (setq data (LM:readcsv file))
        (progn
            (regapp "AcAecLayerStandard")
            (foreach line data
                (setq name      (nth 2 line))
                (setq color     (atoi (nth 6 line)))
                (setq ltype     (nth 7 line))
                (setq plot      (= (nth 11 line) "T"))
                (setq desc      (nth 12 line))
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
            (princ)
        )
    )
)

(setq *defaultParentGroup* "天华标准图层")
(setq *pluginPath* (strcat (getenv "PROGRAMFILES") "\\Autodesk\\ApplicationPlugins\\ThCADPlugin.bundle"))
(setq *pluginContentPath* (strcat *pluginPath* "\\Contents"))

(defun c:THALC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq group "天华建筑标准图层")
    (LM:loadlinetypes '("HIDDEN" "CENTER") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Architecture.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THSLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq group "天华结构标准图层")
    (LM:loadlinetypes '("CENTER" "DASHED" "DASHED2") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Structure.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THMLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq group "天华暖通标准图层")
    (LM:loadlinetypes '("HIDDEN" "CENTER2" "DASHED2") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\HVAC.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THELC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq group "天华电气标准图层")
    (LM:loadlinetypes '("HIDDEN" "HIDDEN2" "DASHDOT" "DASHDOT2" "DIVIDE2" "E-GND" "E-THU" "PHANTOM" "PHANTOM2" "BORDER2" "CENTER" "DIVIDE" "DIVIDE2" "DASHED") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Electrical.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THPLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)

    (setq group "天华给排水标准图层")
    (LM:loadlinetypes '("CENTER2" "BORDER" "DASHED" "DASHDOT") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Plumbing.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun
