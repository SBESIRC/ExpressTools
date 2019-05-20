(defun _make_layer (name colour linetype lineweight plot description)
  (if (null (tblsearch "LAYER" name))
    (progn (regapp "AcAecLayerStandard")
	   (entmake (list 
	   		  '(000 . "LAYER")
			  '(100 . "AcDbSymbolTableRecord")
			  '(100 . "AcDbLayerTableRecord")
			  '(070 . 0)
			  (cons 002 name)
			  (cons   6
				(if (tblsearch "LTYPE" linetype)
					linetype
					"Continuous"
				)
			  )
			  (cons 062 colour)
			  (cons 290 plot)
			  (cons 370 lineweight)
			  (list -3 (list "AcAecLayerStandard" '(1000 . "") (cons 1000 description)))
		    )
	   )
    )
  )
)