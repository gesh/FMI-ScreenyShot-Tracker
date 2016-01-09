<?php

namespace Piss;

use Doctrine\ORM\EntityManager;
use Doctrine\ORM\Tools\Setup;

/**
 * Description of ORM
 *
 * @author x0r
 */
class EmCreator {

    public static function getEm($isDevMode = true) {
        $config = Setup::createAnnotationMetadataConfiguration(array(__DIR__ . "/Entities"), $isDevMode, null, null, false);
        
        $conn = array(
            'driver' => 'pdo_sqlite',
            'path' => __DIR__."/../db/sqlite"
        );

        $entityManager = EntityManager::create($conn, $config);
        return $entityManager;
    }

}
