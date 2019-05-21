(defun TH:loadCSV ( / data file )
    (if
        (and
            (setq file (getfiled "Select CSV File" "" "csv" 16))
            (setq data (LM:readcsv file))
        )
        (progn
	    (regapp "AcAecLayerStandard")
            (foreach line data
			(setq name		(nth 2 line))
	      		(setq color		(atoi (nth 3 line)))
	      		(setq ltype		(nth 4 line))
	     		(setq plot	 	(nth 8 line))
	      		(setq desc		(nth 9 line))
			(MakeLayer name color ltype -3 plot 0 desc)
            )
        )
    )
    (princ)
)

(defun c:THALC ()
 	(TH:loadCSV)
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