;;by 天华AI研究中心
;;天华效率工具平台-通用工具-辅助工具

;Multiple Scale:批量缩放：以各自的开始点（插入点等）为基准点，比例缩放多个实体
(defun c:THMSC(/ num fac i entname entlist bp)
    (princ "\n批量比例缩放\n")
    (setq oldcmd (getvar "cmdecho"))
    (setvar "cmdecho" 0)
    (setq sset (ssget))
    (setq num (sslength sset))
    (setq fac (getreal "\n输入比例系数:"))
    (setq i 0)
    (while (< i num)
        (progn
            (setq entname (ssname sset i))
            (setq entlist (entget entname))
            (setq bp (cdr (assoc 10 entlist)))
            (command "scale" entname "" bp fac)
            (setq i (1+ i))
        );progn
    );while
    (setvar "cmdecho" oldcmd)
);defun

;Z0:Z轴归零：将整幅图所有图元的标高值即z坐标归零
(defun C:THZ0( / *error* oecho );z轴归零
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq ss (ssget "_X"))
    (if (/= ss nil)
        (progn
            (princ "\n正在处理图形数据,请稍候...")
            (command "_.UCS" "");恢复默认坐标系
            (command "_.move" "_all" "" '(0 0 1e99) "" "_.move" "_p" "" '(0 0 -1e99) "")
            (princ "\nOK, 已将所有图元的标高值即Z坐标归零.")
        )
    )
    
    (setvar 'cmdecho oecho)
    (princ)
)
