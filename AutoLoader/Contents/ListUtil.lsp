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

(vl-load-com) (princ)