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
        )
    )
    (princ)
)

(defun c:THALC ()
  	(setq pluginPath (strcat (getenv "PROGRAMFILES") "\\Autodesk\\ApplicationPlugins\\ThCADPlugin.bundle"))
  	(setq pluginContentPath (strcat pluginPath "\\Contents"))
 	(TH:loadCSV (strcat pluginContentPath "\\Standards\\Layer\\Architecture.csv"))
);defun

(defun c:THSLC ()
	(princ)
);defun

(defun c:THMLC ()
	(princ)
);defun

(defun c:THELC ()
	(princ)
);defun

(defun c:THPLC ()
	(princ)
);defun